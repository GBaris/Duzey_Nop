using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.BerkutPay
{
    public class BerkutPaySettings : ISettings
    {
        #region Yapı Kredi

        public bool YKB_IsActive { get; set; }
        public string YKB_MERCHANT_ID { get; set; }
        public string YKB_TERMINAL_ID { get; set; }
        public string YKB_POSNET_ID { get; set; }
        public string YKB_XML_SERVICE_URL { get; set; }
        public string YKB_ENCKEY { get; set; }
        public string YKB_OOS_TDS_SERVICE_URL { get; set; }
        public string YKB_MERCHANT_INIT_URL { get; set; }
        public string YKB_MERCHANT_RETURN_URL { get; set; }
        public bool YKB_OPEN_A_NEW_WINDOW { get; set; }
        public bool YKB_THREE_D { get; set; }
        public bool YKB_PROVISION { get; set; }

        #endregion

        #region Garanti Bankası

        public bool GB_IsActive { get; set; }

        #endregion

    }
}
