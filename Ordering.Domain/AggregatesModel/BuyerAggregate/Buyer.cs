﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ordering.Domain.Events;
using Ordering.Domain.SeedWork;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class Buyer:Entity, IAggregateRoot
    {
        [Required]
        public string IdentityGuid { get; set; }
        public string Name { get; private set; }

        private List<PaymentMethod> _paymentMethods;

        public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

        protected Buyer()
        {
            _paymentMethods = new List<PaymentMethod>();
        }

        public Buyer(string identity, string name): this() 
        {
            IdentityGuid = !string.IsNullOrWhiteSpace(identity) ? identity : throw new ArgumentNullException(nameof(identity));
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public PaymentMethod VerifyOrAddPaymentMethod(
            int cardTypeId,
            string alias,
            string cardNumber,
            string securityNumber,
            string cardHolderName,
            DateTime expiration,
            int orderId)
        {
            var existingPayment = _paymentMethods
                .SingleOrDefault(p => p.EqualTo(cardTypeId, cardNumber, expiration));

            if (existingPayment != null)
            {
                AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));
                return existingPayment;
            }

            var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);
        
            _paymentMethods.Add(payment);

            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

            return payment;
        }
    }
}
