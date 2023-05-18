using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.BerkutPay.Models
{
    internal class SAPResponceModelcs
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public List<Product> Products { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice
        {
            get
            {
                return Products.Sum(p => p.TotalPrice);
            }
        }

        public class Product
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice
            {
                get
                {
                    return Quantity * Price;
                }
            }
        }
    }
}
