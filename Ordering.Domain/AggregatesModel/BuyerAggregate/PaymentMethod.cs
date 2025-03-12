using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ordering.Domain.Exceptions;
using Ordering.Domain.SeedWork;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class PaymentMethod:Entity
    {
        [Required]
        private string _alias;
        [Required]
        private string _cardNumber;
        private string _securityNumber;
        [Required]
        private string _cardHolderName;
        private DateTime _expiration;
        private int _cardTypeId;
        public CardType _cardType { get; private set; }

        public PaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber, string cardHoldName,  DateTime expiration)
        {
            _cardNumber = !string.IsNullOrEmpty(cardNumber) ? cardNumber : throw new OrderingDomainException(nameof(cardNumber));
            _securityNumber = !string.IsNullOrEmpty(securityNumber) ? securityNumber : throw new OrderingDomainException(nameof(securityNumber));
            _cardHolderName = !string.IsNullOrEmpty(cardHoldName) ? cardHoldName : throw new OrderingDomainException(nameof(cardHoldName));
       
            if(expiration < DateTime.UtcNow)
                throw new OrderingDomainException(nameof(expiration));

            _alias = alias;
            _expiration = expiration;
            _cardTypeId = cardTypeId;
        }

        public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
        {
            return _cardTypeId == cardTypeId &&
                   _cardNumber == cardNumber &&
                   _expiration == expiration;
        }

    }
}
