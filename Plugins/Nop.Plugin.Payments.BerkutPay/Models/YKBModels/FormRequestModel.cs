using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.BerkutPay.Models.YKBModels
{
    public record FormRequestModel : BaseNopModel
    {
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Sign { get; set; }
        public string MERCHANT_ID { get; set; }
        public string TERMINAL_ID { get; set; }
        public string POSNET_ID { get; set; }
        public string ENCKEY { get; set; }
        public string OOS_TDS_SERVICE_URL { get; set; }
        public string XML_SERVICE_URL { get; set; }
        public string MERCHANT_INIT_URL { get; set; }
        public string MERCHANT_RETURN_URL { get; set; }
        public bool OPEN_A_NEW_WINDOW { get; set; }
    }

    public record FormResponseModel : BaseNopModel
    {
        public string BankPacket { get; set; }
        public string MerchantPacket { get; set; }
        public string Sign { get; set; }
        public string Xid { get; set; }
        public string Amount { get; set; }
        public string Mac { get; set; }
    }
}
