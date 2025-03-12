using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.UnitTests.Domain
{
    [TestClass]
    internal class BuyerAggregateTest
    {

        public BuyerAggregateTest()
        { }

        [TestMethod]
        public void Create_buyer_item_success()
        {
            //Arrange
            var identity = new Guid().ToString();
            var name = "fakeUser";

            //Act
            var fakeBuyerItem = new Buyer(identity, name);

            //Assert
            Assert.IsNotNull(fakeBuyerItem);
        }

        [TestMethod]
        public void Create_buyer_item_fails()
        {
            //Arrange
            var identity = string.Empty;
            var name = "fakeUser";

            //Act
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                new Buyer(identity, name);
            });
        }

        [TestMethod]
        public void Create_buyer_item_name_fail()
        {
            //Arrange
            var identity = Guid.NewGuid().ToString();
            var name = String.Empty;

            //Act
            Assert.ThrowsException<ArgumentNullException>(() => new Buyer(identity, name))
        }

        [TestMethod]
        public void add_payment_success()
        {
            //Arrange
            var cardTypeId = 1;
            var alias = "fakeAlias";
            var cardNumber = "124";
            var securityNumber = "1234";
            var cardHolderName = "FakeHolderName";
            var expiration = DateTime.UtcNow.AddYears(1);
            var orderId = 1;
            var name = "fakeUser";
            var identity = new Guid().ToString();
            var fakeBuyerItem = new Buyer(identity, name);

            //Act
            var result = fakeBuyerItem.VerifyOrAddPaymentMethod(
                cardTypeId,
                alias,
                cardNumber,
                securityNumber,
                cardHolderName,
                expiration,
                orderId);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void create_payment_method_success()
        {
            //Arrange
            var cardTypeId = 1;
            var alias = "fakeAlias";
            var cardNumber = "124";
            var securityNumber = "1234";
            var cardHolderName = "FakeHolderNAme";
            var expiration = DateTime.UtcNow.AddYears(1);

            //Act
            var fakePaymentMethod = new PaymentMethod(
                cardTypeId,
                alias,
                cardNumber,
                securityNumber,
                cardHolderName,
                expiration);

           //Assert
           Assert.IsNotNull(fakePaymentMethod);
        }


        [TestMethod]
        public void create_payment_method_expiration_fail()
        {
            //Arrange
            var cardTypeId = 1;
            var alias = "FakeAlias";
            var cardNumber = "124";
            var securityNumer = "1234";
            var cardHolderName = "FakeHolderName";
            var expiration = DateTime.UtcNow.AddYears(-1);


            //Act - Assert
            Assert.ThrowsException<OrderingDomainException>(() => {
                new PaymentMethod(
                    cardTypeId,
                    alias,
                    cardNumber,
                    securityNumer,
                    cardHolderName,
                    expiration);
                    });                
        }

        [TestMethod]
        public void payment_method_isEqualTo()
        {
            //Arrange
            var cardTypeId = 1;
            var alias = "fakeAlias";
            var cardNumber = "124";
            var securityNumber = "1234";
            var cardHolderName = "FakeHolderName";
            var expiration = DateTime.UtcNow.AddYears(1);

            //Act
            var fakePaymentMethod = new PaymentMethod(
                cardTypeId,
                alias,
                cardNumber,
                securityNumber,
                cardHolderName,
                expiration);

            var result =
                fakePaymentMethod.IsEqualTo(cardTypeId,
                cardNumber,
                expiration);

            //Assert
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Add_new_PaymentMethod_raises_new_event()
        {
            //Arrange
            var alias = "fakeAlias";
            var orderId = 1;
            var cardTypeId = 5;
            var cardNumber = "12";
            var cardSecurityNumber = "123";
            var cardHolderName = "FakeName";
            var cardExpiaration = DateTime.UtcNow.AddYears(1);
            var expectedResult = 1;
            var name = "fakeUser";

            //Act
            var fakeUser = new Buyer(Guid.NewGuid().ToString(),
                name);
            fakeUser.VerifyOrAddPaymentMethod(
                cardTypeId,
                alias,
                cardNumber,
                cardSecurityNumber,
                cardHolderName,
                cardExpiaration,
                orderId);

            //Assert
            Assert.AreEqual(fakeUser.DomainEvents.Count, expectedResult);
        }

    }
}
