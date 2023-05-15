using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Payments.BerkutPay.Data.Domain;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;

namespace Nop.Plugin.Payments.BerkutPay.Services
{
    public class CardService : ICardService
    {
        private readonly IRepository<Card> _cardRepository;
        public CardService(IRepository<Card> cardRepository)
        {
            _cardRepository = cardRepository;
        }
        public Card GetCardByPrefix(string prefixNumber)
        {
            var card = _cardRepository.Table
                           .FirstOrDefault(c => c.PrefixNo == prefixNumber);
            return card;
        }
    }
}
