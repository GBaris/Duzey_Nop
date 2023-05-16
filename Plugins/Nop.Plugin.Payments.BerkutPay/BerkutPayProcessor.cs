using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.BerkutPay.Components;
using Nop.Plugin.Payments.BerkutPay.Models;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;
using Nop.Plugin.Payments.BerkutPay.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.BerkutPay
{
    public class BerkutPayProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly BerkutPaySettings _berkutPaySettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IYKB_Service _ykbService;


        #endregion

        #region Ctor

        public BerkutPayProcessor(
            IWebHelper webHelper, 
            ISettingService settingService,
            ILocalizationService localizationService, 
            BerkutPaySettings berkutPaySettings,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService, 
            IYKB_Service ykbService
            )
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _berkutPaySettings = berkutPaySettings;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _ykbService = ykbService;
        }

        #endregion

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (_berkutPaySettings.YKB_IsActive)
            {
                if (_berkutPaySettings.YKB_THREE_D)
                {
                    if (_berkutPaySettings.YKB_PROVISION)
                    {
                        return await _ykbService.ProcessPayment3DAuthAsync(processPaymentRequest);
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (_berkutPaySettings.YKB_PROVISION)
                    {

                    }
                    else
                    {

                    }
                }
            }
            else if (_berkutPaySettings.GB_IsActive)
            {
                throw new NotImplementedException();
            }

            // If none of the conditions are met, return null or handle it based on your requirement
            return null;
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            if (_berkutPaySettings.YKB_IsActive)
            {
                if (_berkutPaySettings.YKB_THREE_D)
                {
                    if (_berkutPaySettings.YKB_PROVISION)
                    {
                        if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                        {
                            await _orderProcessingService.MarkAsAuthorizedAsync(order);
                            return;
                        }
                    }
                    else
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                            return;
                        }
                    }
                }
                else
                {
                    if (_berkutPaySettings.YKB_PROVISION)
                    {
                        if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                        {
                            await _orderProcessingService.MarkAsAuthorizedAsync(order);
                            return;
                        }
                    }
                    else
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                            return;
                        }
                    }
                }
            }
            else if (_berkutPaySettings.GB_IsActive)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("No payment method is active.");
            }
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        async Task<decimal> IPaymentMethod.GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, 0, false);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            });
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Nop.Plugin.Payments.BerkutPay.Fields.Berkut.PaymentMethodDescription");

        }

        public Type GetPublicViewComponent()
        {
            return typeof(BerkutPayViewComponent);
        }

        Task<bool> IPaymentMethod.HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }


        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return Task.FromResult<IList<string>>(warnings);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(false);
        }


        #region Overrides

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BerkutPay/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new BerkutPaySettings
            {
                #region Yapı Kredi

                YKB_IsActive = false,
                YKB_MERCHANT_ID = "",
                YKB_TERMINAL_ID = "",
                YKB_POSNET_ID = "",
                YKB_XML_SERVICE_URL = "",
                YKB_ENCKEY = "",
                YKB_OOS_TDS_SERVICE_URL = "",
                YKB_MERCHANT_INIT_URL = "",
                YKB_MERCHANT_RETURN_URL = "",
                YKB_OPEN_A_NEW_WINDOW = false,
                YKB_THREE_D = false,
                YKB_PROVISION = false,

                #endregion

                #region Garanti Bankasi

                GB_IsActive = false,

                #endregion

            };
            await _settingService.SaveSettingAsync(settings);


            //locales

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {

                ["Nop.Plugin.Payments.BerkutPay.Fields.Berkut.PaymentMethodDescription"] = "Banka / Kredi Kartı"

            });

            #region Yapı Kredi

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Nop.Plugin.Payments.BerkutPay.Fields.Berkut.YKB_IsActive"] = "Yapı Kredi",
                ["Nop.Plugin.Payments.BerkutPay.Fields.Berkut.YKB_IsActive.hint"] = "Yapı Kredi Pos entegrasyonu aktif edilsin mi?",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_Instructions"] = "Yapı Kredi Sanal Pos Entegrasyon Bilgileri",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_ID"] = "MERCHANT ID",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_ID.Hint"] = "10 haneli YKB üye işyei numarası",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_TERMINAL_ID"] = "TERMINAL ID",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_TERMINAL_ID.Hint"] = "8 haneli YKB üye işyeri terminal numarası",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_POSNET_ID"] = "POSNET ID",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_POSNET_ID.Hint"] = "16 haneye kadar YKB üye işyeri POSNET numarası",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_ENCKEY"] = "ENCKEY",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_ENCKEY.Hint"] = "Şifreleme anahtarı",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_OOS_TDS_SERVICE_URL"] = "OOS TDS SERVICE URL",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_OOS_TDS_SERVICE_URL.Hint"] = "Form yönlendirmesi yapılacak banka sayfası adresi",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_XML_SERVICE_URL"] = "XML SERVICE URL",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_XML_SERVICE_URL.Hint"] = "Banka entegrasyon servis adresi",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_INIT_URL"] = "MERCHANT INIT URL",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_INIT_URL.Hint"] = "Üye işyerinin işlem yaptığı web sitesi adresi",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_RETURN_URL"] = "MERCHANT RETURN URL",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_RETURN_URL.Hint"] = "Form yönlendirmesinin geri yapılacağı işyeri sayfa adresi. Max 255 karakter",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_OPEN_A_NEW_WINDOW"] = "OPEN A NEW WINDOW",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_OPEN_A_NEW_WINDOW.Hint"] = "POST edilecek formun yeni bir sayfaya mı yoksa mevcut sayfayı mı yönlendirileceğini belirt",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_THREE_D"] = "3D Secure",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_THREE_D.Hint"] = "3D Doğrulama için yönlendirme",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_PROVISION"] = "Provision",
                ["Nop.Plugin.Payments.BerkutPay.Fields.YKB_PROVISION.Hint"] = "Provizyonlu satış / Direkt satış"
            });

            #endregion

            #region Garanti Bankası

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Nop.Plugin.Payments.BerkutPay.Fields.Berkut.GB_IsActive"] = "Garanti",
                ["Nop.Plugin.Payments.BerkutPay.Fields.Berkut.GB_IsActive.hint"] = "Garanti Bankası Pos entegrasyonu aktif edilsin mi?"
            });

            #endregion

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<BerkutPaySettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.BerkutPay");

            await base.UninstallAsync();
        }

        #endregion


        #region Properties
        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        #endregion

    }
}
