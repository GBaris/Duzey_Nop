using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;


namespace Nop.Plugin.Payments.BerkutPay.Models
{
    public record class BerkutPayConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        #region Yapı Kredi

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.Berkut.YKB_IsActive")]
        public bool YKB_IsActive { get; set; }
        public bool YKB_IsActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_ID")]
        public string YKB_MERCHANT_ID { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_TERMINAL_ID")]
        public string YKB_TERMINAL_ID { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_POSNET_ID")]
        public string YKB_POSNET_ID { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_XML_SERVICE_URL")]
        public string YKB_XML_SERVICE_URL { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_ENCKEY")]
        public string YKB_ENCKEY { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_OOS_TDS_SERVICE_URL")]
        public string YKB_OOS_TDS_SERVICE_URL { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_INIT_URL")]
        public string YKB_MERCHANT_INIT_URL { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_MERCHANT_RETURN_URL")]
        public string YKB_MERCHANT_RETURN_URL { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_OPEN_A_NEW_WINDOW")]
        public bool YKB_OPEN_A_NEW_WINDOW { get; set; }
        public bool YKB_OPEN_A_NEW_WINDOW_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_THREE_D")]
        public bool YKB_THREE_D { get; set; }
        public bool YKB_THREE_D_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_PROVISION")]
        public bool YKB_PROVISION { get; set; }
        public bool YKB_PROVISION_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_INSTALLMENT_IsActive")]
        public bool YKB_INSTALLMENT_IsActive { get; set; }
        public bool YKB_INSTALLMENT_IsActive_OverrideForStore { get;set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_INSTALLMENT_2")]
        public bool YKB_INSTALLMENT_2 { get; set; }
        public bool YKB_INSTALLMENT_2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_INSTALLMENT_3")]
        public bool YKB_INSTALLMENT_3 { get; set; }
        public bool YKB_INSTALLMENT_3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_INSTALLMENT_4")]
        public bool YKB_INSTALLMENT_4 { get; set; }
        public bool YKB_INSTALLMENT_4_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.YKB_INSTALLMENT_6")]
        public bool YKB_INSTALLMENT_6 { get; set; }
        public bool YKB_INSTALLMENT_6_OverrideForStore { get; set; }

        #endregion

        #region Garanti Bankasi

        [NopResourceDisplayName("Nop.Plugin.Payments.BerkutPay.Fields.Berkut.GB_IsActive")]
        public bool GB_IsActive { get; set; }
        public bool GB_IsActive_OverrideForStore { get; set; }

        #endregion
    }
}
