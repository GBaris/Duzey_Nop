using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.BerkutPay.Models;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
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
        private readonly ICardService _cardService;

        #endregion

        #region Ctor

        public BerkutPayController(
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICardService cardService
            )
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _cardService = cardService;

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
                YKB_PROVISION = settings.YKB_PROVISION,
                YKB_INSTALLMENT_IsActive= settings.YKB_INSTALLMENT_IsActive,
                YKB_INSTALLMENT_2 = settings.YKB_INSTALLMENT_2,
                YKB_INSTALLMENT_3 = settings.YKB_INSTALLMENT_3,
                YKB_INSTALLMENT_4 = settings.YKB_INSTALLMENT_4,
                YKB_INSTALLMENT_6 = settings.YKB_INSTALLMENT_6,

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
                YKB_OPEN_A_NEW_WINDOW = model.YKB_OPEN_A_NEW_WINDOW != false ? model.YKB_OPEN_A_NEW_WINDOW : existingSettings.YKB_OPEN_A_NEW_WINDOW,
                YKB_THREE_D = model.YKB_THREE_D != false ? model.YKB_THREE_D : existingSettings.YKB_THREE_D,
                YKB_PROVISION = model.YKB_PROVISION != false ? model.YKB_PROVISION : existingSettings.YKB_PROVISION,
                YKB_INSTALLMENT_IsActive = model.YKB_INSTALLMENT_IsActive != false ? model.YKB_INSTALLMENT_IsActive : existingSettings.YKB_INSTALLMENT_IsActive,
                YKB_INSTALLMENT_2 = model.YKB_INSTALLMENT_2 != false ? model.YKB_INSTALLMENT_2 : existingSettings.YKB_INSTALLMENT_2,
                YKB_INSTALLMENT_3 = model.YKB_INSTALLMENT_3 != false ? model.YKB_INSTALLMENT_3 : existingSettings.YKB_INSTALLMENT_3,
                YKB_INSTALLMENT_4 = model.YKB_INSTALLMENT_4 != false ? model.YKB_INSTALLMENT_4 : existingSettings.YKB_INSTALLMENT_4,
                YKB_INSTALLMENT_6 = model.YKB_INSTALLMENT_6 != false ? model.YKB_INSTALLMENT_6 : existingSettings.YKB_INSTALLMENT_6,


                #endregion
            };

            await _settingService.SaveSettingAsync(settings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return View("~/Plugins/Payments.BerkutPay/Views/Configure.cshtml", model);
        }


        //BIN nımarasıyla card kontrolü
        public ActionResult GetCardInfo(string prefixNumber)
        {
            var card = _cardService.GetCardByPrefix(prefixNumber);
            if (card != null)
            {
                return Json(new
                {
                    bankId = card.BankId,
                    cardType = card.Type,
                    isSuccess = true
                });
            }
            return Json(new
            {
                isSuccess = false
            });
        }

        [HttpPost]
        public IActionResult ThreeDEnableIsActive(bool use3DSecure)
        {
            // Kullanıcının seçimini bir yerde saklayın veya işleyin.
            // Bu durumda, ProcessPaymentRequest'deki CustomValues'a ekliyoruz.

            // Öncelikle bir ProcessPaymentRequest örneği oluşturmanız veya mevcut bir örneği elde etmeniz gerekmektedir.
            ProcessPaymentRequest paymentRequest = new ProcessPaymentRequest();

            if (paymentRequest.CustomValues.ContainsKey("Use3DSecure"))
            {
                paymentRequest.CustomValues["Use3DSecure"] = use3DSecure;
            }
            else
            {
                paymentRequest.CustomValues.Add("Use3DSecure", use3DSecure);
            }

            return Json(new { success = true });
        }
    }
}
