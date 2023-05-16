using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Payments.BerkutPay.Models.YKBModels;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.BerkutPay.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [ValidateAntiForgeryToken]
    [AutoValidateAntiforgeryToken]
    [ValidateVendor]
    [SaveSelectedTab]
    public class YKBController : BasePaymentController
    {

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly IYKB_Service _ykbService;

        #endregion

        #region Ctor

        public YKBController(ILocalizationService localizationService,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IYKB_Service ykbService
            )
        {
            _localizationService = localizationService;
            _storeContext = storeContext;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _ykbService = ykbService;
        }

        #endregion

        #region II. Aşama

        //Burada 3D doğrulamadan gelen veriler çözümleniyor

        [HttpPost]
        public async Task<IActionResult> ThreeDRedirectAsync(IFormCollection form)
        {
            if (_ykbService.IsValidForm(form))
            {
                var bankData = form["BankPacket"];
                var merchantData = form["MerchantPacket"];
                var sign = form["Sign"];
                var xid = form["Xid"];
                var amount = form["Amount"];
                var mac = _ykbService.GetMacData(xid, amount);

                return RedirectToAction("ThreeDRedirectGet", new FormResponseModel
                {
                    BankPacket = bankData,
                    MerchantPacket = merchantData,
                    Mac = mac,
                    Sign = sign,
                    Xid = xid,
                    Amount = amount
                });
            }
            string routeName = await _ykbService.HandlePaymentErrorAsync();
            return RedirectToRoute(routeName);
        }

        [HttpGet]
        public async Task<IActionResult> ThreeDRedirectGet(FormResponseModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (await _ykbService.IsCheckoutDisabledOrCartEmpty(customer, store))
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (await _ykbService.IsOnePageCheckoutEnabledOrGuestNotAllowedAsync(customer))
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (!await _ykbService.IsMinimumOrderPlacementIntervalValidAsync(customer))
            {
                throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));
            }

            var processPaymentRequest = _ykbService.PrepareProcessPaymentRequest(model, customer, store);
            var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                await _ykbService.HandleSuccessfulOrderAsync(placeOrderResult.PlacedOrder);
                return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
            }
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        #endregion

    }
}