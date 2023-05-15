using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.Payments.BerkutPay.Data.Domain
{
    public class Bank : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<Card> Cards { get; set; }
    }
}
