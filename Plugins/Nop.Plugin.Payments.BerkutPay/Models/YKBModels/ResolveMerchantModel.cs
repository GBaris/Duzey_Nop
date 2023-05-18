using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.BerkutPay.Models.YKB_Models
{
    public record ResolveMerchantModel : BaseNopModel
    {
        public string Xid { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Installment { get; set; }
        public string Point { get; set; }
        public string PointAmount { get; set; }
        public string TxStatus { get; set; }
        public string MdStatus { get; set; }
        public string MdErrorMessage { get; set; }
        public string Mac { get; set; }
        public string ErrorMessage { get; internal set; }
    }
}
