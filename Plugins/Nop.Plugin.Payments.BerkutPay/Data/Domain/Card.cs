using Nop.Core;

namespace Nop.Plugin.Payments.BerkutPay.Data.Domain
{

    public class Card : BaseEntity
    {
        public string PrefixNo { get; set; }
        public CardType Type { get; set; }
        public bool IsBusinessCard { get; set; }
        public int BankId { get; set; }
        public Bank Bank { get; set; }
    }

    public enum CardType
    {
        CreditCard = 0,
        DebitCard = 1,
        PrepaidCard = 2
    }
}
