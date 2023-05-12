using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Payments.BerkutPay.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
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
    public class BerkutPayController : BasePluginController
    {

        #region Fields

        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<Order> _orderRepository;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BerkutPaySettings _berkutPaySettings;

        #endregion

        #region Ctor

        public BerkutPayController(
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IRepository<Order> orderRepository,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IOrderService orderService,
            OrderSettings orderSettings,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            BerkutPaySettings berkutPaySettings
            )
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _orderRepository = orderRepository;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _orderService = orderService;
            _orderSettings = orderSettings;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _paymentService = paymentService;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _berkutPaySettings = berkutPaySettings;
        }

        #endregion


        public async Task<IActionResult> Configure()
        {
            var settings = await _settingService.LoadSettingAsync<BerkutPaySettings>();
            var model = new BerkutPayConfigurationModel
            {
                YKB_IsActive = settings.YKB_IsActive,

                #region Yapı Kredi

                YKB_MERCHANT_ID = settings.YKB_MERCHANT_ID,
                YKB_TERMINAL_ID = settings.YKB_TERMINAL_ID,
                YKB_POSNET_ID = settings.YKB_POSNET_ID,
                YKB_XML_SERVICE_URL = settings.YKB_XML_SERVICE_URL,
                YKB_ENCKEY = settings.YKB_ENCKEY,
                YKB_OOS_TDS_SERVICE_URL = settings.YKB_OOS_TDS_SERVICE_URL,
                YKB_MERCHANT_INIT_URL = settings.YKB_MERCHANT_INIT_URL,
                YKB_MERCHANT_RETURN_URL = settings.YKB_MERCHANT_RETURN_URL,
                YKB_OPEN_A_NEW_WINDOW = settings.YKB_OPEN_A_NEW_WINDOW,
                YKB_THREE_D = settings.YKB_THREE_D,

                #endregion
            };
            return View("~/Plugins/Payments.BerkutPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(BerkutPayConfigurationModel model)
        {
            var existingSettings = await _settingService.LoadSettingAsync<BerkutPaySettings>();

            var settings = new BerkutPaySettings
            {
                YKB_IsActive = model.YKB_IsActive,
                GB_IsActive = model.GB_IsActive,

                #region Yapı Kredi

                YKB_MERCHANT_ID = !string.IsNullOrEmpty(model.YKB_MERCHANT_ID) ? model.YKB_MERCHANT_ID : existingSettings.YKB_MERCHANT_ID,
                YKB_TERMINAL_ID = !string.IsNullOrEmpty(model.YKB_TERMINAL_ID) ? model.YKB_TERMINAL_ID : existingSettings.YKB_TERMINAL_ID,
                YKB_POSNET_ID = !string.IsNullOrEmpty(model.YKB_POSNET_ID) ? model.YKB_POSNET_ID : existingSettings.YKB_POSNET_ID,
                YKB_XML_SERVICE_URL = !string.IsNullOrEmpty(model.YKB_XML_SERVICE_URL) ? model.YKB_XML_SERVICE_URL : existingSettings.YKB_XML_SERVICE_URL,
                YKB_OOS_TDS_SERVICE_URL = !string.IsNullOrEmpty(model.YKB_OOS_TDS_SERVICE_URL) ? model.YKB_OOS_TDS_SERVICE_URL : existingSettings.YKB_OOS_TDS_SERVICE_URL,
                YKB_ENCKEY = !string.IsNullOrEmpty(model.YKB_ENCKEY) ? model.YKB_ENCKEY : existingSettings.YKB_ENCKEY,
                YKB_MERCHANT_INIT_URL = !string.IsNullOrEmpty(model.YKB_MERCHANT_INIT_URL) ? model.YKB_MERCHANT_INIT_URL : existingSettings.YKB_MERCHANT_INIT_URL,
                YKB_MERCHANT_RETURN_URL = !string.IsNullOrEmpty(model.YKB_MERCHANT_RETURN_URL) ? model.YKB_MERCHANT_RETURN_URL : existingSettings.YKB_MERCHANT_RETURN_URL,
                YKB_OPEN_A_NEW_WINDOW = model.YKB_OPEN_A_NEW_WINDOW,
                YKB_THREE_D = model.YKB_THREE_D,
                YKB_PROVISION = model.YKB_PROVISION,

                #endregion
            };

            await _settingService.SaveSettingAsync(settings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return View("~/Plugins/Payments.BerkutPay/Views/Configure.cshtml", model);
        }


    }
}
