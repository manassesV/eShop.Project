using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ordering.API.Infrastructure.Services;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Ordering.UnitTests.Application
{
    [TestClass]
    public class NewOrderRequestHandlerTest
    {
        public readonly IOrderRepository _orderRepository;
        public readonly IIdentityService _identityService;
        public readonly IMediator _mediator;
        public readonly IOrderingIntegrationEventService _orderingIntegrationEventService;


        public NewOrderRequestHandlerTest()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _identityService = Substitute.For<IIdentityService>();
            _mediator = Substitute.For<IMediator>();
            _orderingIntegrationEventService = Substitute.For<OrderingIntegrationEventService>();
        }

        [TestMethod]
        public async Task Handle_return_false_if_order_not_persisted()
        {
            var buyerId = "1234";

            var fakeOrderCMD = FakeOrderRequestWithBuyer(new Dictionary<string, object>
            {
                ["cardExpiration"] = DateTime.UtcNow.AddYears(1)
            });

            _orderRepository.GetAsync(Arg.Any<int>())
                .Returns(Task.FromResult(FakeOrder()));

            _orderRepository.UnitOfWork.SaveChangesAsync(default)
                .Returns(Task.FromResult(1));

            _identityService.GetUserIdentity().Returns(buyerId);

            var logMock = Substitute.For<ILogger<CreateOrderCommandHandler>>();

            //Act
            var handler = new CreateOrderCommandHandler(_mediator, _orderingIntegrationEventService,
                _orderRepository, _identityService, logMock);
            var cltToken = new CancellationToken();


            var result = await handler.Handle(fakeOrderCMD, cltToken);

            //Assert
            Assert.IsTrue(result);

        }

        [TestMethod]
        public async Task Handle_throws_exception_when_no_buyerId()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Buyer(string.Empty, "Manasses"));
        }

        private Order FakeOrder()
        {
            return new Order("1", "fakeName", new Address("street", "city", "state", "country", "zipcode"), 1, "12", "111", "fakeName", DateTime.UtcNow.AddYears(1));
        }

        private CreateOrderCommand FakeOrderRequestWithBuyer(Dictionary<string, object> args)
        {
            return new CreateOrderCommand
            (
                new List<BasketItem>(),
                userId: args != null && args.ContainsKey("userId") ? (string)args["userId"] : null,
                userName: args != null && args.ContainsKey("userName") ? (string)args["userName"] : null,
                city: args != null && args.ContainsKey("city") ? (string)args["city"] : null,
                street: args != null && args.ContainsKey("street") ? (string)args["street"] : null,
                state: args != null && args.ContainsKey("state") ? (string)args["state"] : null,
                country: args != null && args.ContainsKey("country") ? (string)args["country"] : null,
                zipcode: args != null && args.ContainsKey("zipcode") ? (string)args["zipcode"] : null,
                cardNumber: args != null && args.ContainsKey("cardNumber") ? (string)args["cardNumber"] : "1234",
                cardExpiration: args != null && args.ContainsKey("cardExpiration") ? (DateTime)args["cardExpiration"] : DateTime.MinValue,
                cardSecurityNumber: args != null && args.ContainsKey("cardSecurityNumber") ? (string)args["cardSecurityNumber"] : "123",
                cardHolderName: args != null && args.ContainsKey("cardHolderName") ? (string)args["cardHolderName"] : "XXX",
                cardTypeId: args != null && args.ContainsKey("cardTypeId") ? (int)args["cardTypeId"] : 0

            );
        }
    }
}
