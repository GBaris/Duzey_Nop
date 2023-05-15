using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Payments.BerkutPay.Data.Domain;

namespace Nop.Plugin.Payments.BerkutPay.Services.IServices
{
    public interface ICardService
    {
        Card GetCardByPrefix(string prefixNumber);
    }
}
