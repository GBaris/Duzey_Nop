using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.BerkutPay.Models.YKB_Models
{
    public record OOSRequestModel : BaseNopModel
    {
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Sign { get; set; }

    }
}
