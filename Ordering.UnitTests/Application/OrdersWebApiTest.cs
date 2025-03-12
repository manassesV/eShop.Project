using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Ordering.API.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ordering.UnitTests.Application
{
    [TestClass]
    public class OrdersWebApiTest
    {
        private IMediator _mediator;
        private IOrderQueries _orderQueries;
        private IIdentityService _identityService;
        private ILogger<OrderServices> _logger;

        public OrdersWebApiTest()
        {
            _mediator = Substitute.For<IMediator>();
            _orderQueries = Substitute.For<IOrderQueries>();
            _identityService = Substitute.For<IIdentityService>();
            _logger = Substitute.For<ILogger<OrderServices>>();
        }

        [TestMethod]
        public async Task cancel_order_with_requesid_success()
        {
            //Arrange
            _mediator.Send(Arg.Any<IdentifiedCommand<CancelOrderCommand, bool>>(), default).
                Returns(Task.FromResult(true));

            //Act
            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);
            var result = OrdersApi.CancelOrderAsync(new Guid(), new CancelOrderCommand(1), orderServices);


            //Assert
            Assert.IsInstanceOfType<Ok>(result.Result);
        }

        [TestMethod]
        public async Task cancel_order_with_bad_request()
        {
            //Arrange
            _mediator.Send(Arg.Any<IdentifiedCommand<CancelOrderCommand, bool>>(), default).
                Returns(Task.FromResult(false));

            //act
            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);
            var result = OrdersApi.CancelOrderAsync(Guid.Empty, new CancelOrderCommand(1), orderServices);

            //Assert
            Assert.IsInstanceOfType<BadRequest>(result.Result);

        }

        [TestMethod]
        public async Task ship_order_with_requestid_sucess()
        {
            //Arrange
            _mediator.Send(Arg.Any<IdentifiedCommand<ShipOrderCommand, bool>>(), default)
                .Returns(Task.FromResult(true));

            //Act
            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);
            var result = OrdersApi.ShipOrderAsync(Guid.NewGuid(), new ShipOrderCommand(1), orderServices);

            //Assert
            Assert.IsInstanceOfType<Ok>(result?.Result);
        }


        [TestMethod]
        public async Task ship_order_with_requestid_bad_request()
        {
            //Arrange
            _mediator.Send(Arg.Any<IdentifiedCommand<ShipOrderCommand, bool>>(), default)
                .Returns(Task.FromResult(false));

            //Act
            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);
            var result = OrdersApi.ShipOrderAsync(Guid.Empty, new ShipOrderCommand(1), orderServices);

            //Assert
            Assert.IsInstanceOfType<BadRequest>(result.Result);
        }

        [TestMethod]
        public async Task Get_orders_success()
        {
            //Arrange
            var fakeDinamic = Enumerable.Empty<OrderSummary>();

            _identityService.GetUserIdentity().
                Returns(Guid.NewGuid().ToString());

            _orderQueries.GetOrdersFromUserAsync(Guid.NewGuid().ToString()).
                Returns(Task.FromResult(fakeDinamic));

            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);

            var result = OrdersApi.GetOrdersByUserAsync(orderServices);

            //Assert
            Assert.IsInstanceOfType<Ok<IEnumerable<OrderSummary>>>(result.Result);
        }

        [TestMethod]
        public async Task Get_order_success()
        {
            //Arrange
            var fakeOrderId = 123;
            var fakeDynamicResult = new Order();

            _orderQueries.GetOrderAsync(Arg.Any<int>())
                .Returns(Task.FromResult(fakeDynamicResult));

            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);

            var result = await OrdersApi.GetOrderAsync(fakeOrderId, orderServices);

            //Assert
            Assert.IsInstanceOfType<Ok<Order>>(result.Result);
            Assert.AreSame(fakeDynamicResult, ((Ok<Order>)result.Result).Value);

        }

        [TestMethod]
        public async Task Get_order_fails()
        {
            //Arrange
            var fakeOrderId = 123;

            _orderQueries.GetOrderAsync(Arg.Any<int>()).
                Throws(new KeyNotFoundException());


            var orderServices = new OrderServices(_mediator, _orderQueries, _identityService, _logger);
            var result = await OrdersApi.GetOrderAsync(fakeOrderId, orderServices);

            Assert.IsInstanceOfType<NotFound>(result.Result);
        }

        [TestMethod]
        public async Task Get_card_type_success()
        {
            var fakeCardType = Enumerable.Empty<Cardtype>();

            _orderQueries.GetCardTypesAsync()
                 .Returns(Task.FromResult(fakeCardType));

            var result = await OrdersApi.GetCardTypesAsync(_orderQueries);

            Assert.IsInstanceOfType<Ok<IEnumerable<Cardtype>>>(result);

            Assert.AreSame(fakeCardType, result.Value);
        }

        [TestMethod]
        public void Set_Stock_Rejected_Order_check_Serialization()
        {
            // Arrange
            var command = new SetStockRejectedOrderStatusCommand(123, new List<int> { 1, 2, 3 });

            // Act
            var json = JsonSerializer.Serialize(command);
            var deserializedCommand = JsonSerializer.Deserialize<SetStockRejectedOrderStatusCommand>(json);

            //Assert
            Assert.AreEqual(command.OrderNumber, deserializedCommand.OrderNumber);

            //Assert for List<int>
            Assert.IsNotNull(deserializedCommand.OrderStockItems);
            Assert.AreEqual(command.OrderStockItems.Count, deserializedCommand.OrderStockItems.Count);

            for (int i = 0; i < command.OrderStockItems.Count; i++)
            {
                Assert.AreEqual(command.OrderStockItems[i], deserializedCommand.OrderStockItems[i]);
            }
        }
    }
}
