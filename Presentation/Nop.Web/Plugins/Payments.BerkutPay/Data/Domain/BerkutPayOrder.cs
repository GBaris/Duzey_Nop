using System;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Nop.Core;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Payments.BerkutPay.Data.Domain
{
    public class BerkutPayOrder : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string MerchantPacket { get; set; }
        public string BankPacket { get; set; }
        public string Sign { get; set; }
        public string OrderGuid { get; set; } 
        public string TransactionResult { get; set; }

    }
}
