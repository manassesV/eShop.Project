using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.Http;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Ordering.FunctionalTests;

public sealed class OrderingApiTests: IClassFixture<OrderingApiFixture>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _httpClient;

    public OrderingApiTests(OrderingApiFixture fixture)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new ApiVersion(1.0));

        _webApplicationFactory = fixture;
        _httpClient = _webApplicationFactory.CreateDefaultClient(handler);
    }

    [Fact]
    public async Task GetAllStoredOrdersWorks()
    {
        //Act
        var response = await _httpClient.GetAsync("api/orders");
        var s = response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelWithEmptyGuidFails()
    {
        //Act
        var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.Empty.ToString() } }
        };
        var response = await _httpClient.PutAsync("/api/orders/cancel", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelNonExistentOrderFails()
    {
        //Act
        var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.NewGuid().ToString() } }
        };

        var response = await _httpClient.PutAsync("api/orders/cancel", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ShipWithEmptyGuidFails()
    {
        //Acts 
        var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.Empty.ToString() } }
        };

        var response = await _httpClient.PutAsync("api/orders/ship", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShipNonExistentOrderFails()
    {
        //Act
        var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.NewGuid().ToString() } }
        };

        var response = await _httpClient.PutAsync("api/orders/ship", content);

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetAllOrdersCardType()
    {
        //Act 1
        var response = await _httpClient.GetAsync("api/orders/cardtypes");
        var s = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStoredOrdersWithOrderId()
    {
        //Act
        var response = await _httpClient.GetAsync("api/orders/1");
        var responseStatus = response.StatusCode;

        //Assert
        Assert.Equal("NotFound", responseStatus.ToString());
    }

    [Fact]
    public async Task AddNewEmptyOrder()
    {
        //Act
        var content = new StringContent(JsonSerializer.Serialize(new Order()), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.Empty.ToString() } }
        };
        var response = await _httpClient.PostAsync("api/orders", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }
    [Fact]
    public async Task AddNewOrder()
    {
        //Act
        var item = new BasketItem()
        {
            Id = "1",
            ProductId = 12,
            ProductName = "Test",
            UnitPrice = 10,
            OldUnitPrice = 9,
            Quantity = 1,
            PictureUrl = null
        };

        var cardExpirationDate = Convert.ToDateTime("2023-12-22T12:34:24.334Z");
        var OrderRequest = new CreateOrderRequest("1", "TestUser", null, null, null, null, null, "XXXXXXXXXXXX0005", "Test User", cardExpirationDate, "test buyer", 1, null, new List<BasketItem> { item });
        var content = new StringContent(JsonSerializer.Serialize(OrderRequest), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.NewGuid().ToString() } }
        };

        var response = await _httpClient.PostAsync("api/orders", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostDraftOrder()
    {
        //Act
        var item = new BasketItem
        {
            Id = "1",
            ProductId = 12,
            ProductName = "Test",
            UnitPrice = 10,
            OldUnitPrice = 9,
            Quantity = 1,
            PictureUrl = null
        };

        var bodyContent = new CustomerBasket("1", new List<BasketItem> { item });
        var content = new StringContent(JsonSerializer.Serialize(bodyContent), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "requestid", Guid.NewGuid().ToString() } }
        };

        var response = await _httpClient.PostAsync("api/orders/draft", content);
        var s = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }

    [Fact]
    public async Task CreateOrderDraftSucceeds()
    {
        var payload = FakeOrderDrafCommand();
        var content = new StringContent(JsonSerializer.Serialize(FakeOrderDrafCommand()), UTF8Encoding.UTF8, "application/json")
        {
            Headers = { { "x-requestid", Guid.NewGuid().ToString() } }
        };

        var response = await _httpClient.PostAsync("api/orders/draft", content);

        var s = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<OrderDraftDTO>(s, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(payload.Items.Count(), responseData.OrderItems.Count());
        Assert.Equal(payload.Items.Sum(o => o.Quantity * o.UnitPrice), responseData.Total);
        AssertThatOrderItemsAreTheSameAsRequestPayloadItems(payload, responseData);

    }

    private static void AssertThatOrderItemsAreTheSameAsRequestPayloadItems(CreateOrderDraftCommand payload, OrderDraftDTO responseData)
    {
        //check that OrderItems contain all product Ids from Ids from the payload
        var payloadItemsProductIds = payload.Items.Select(o => o.ProductId);
        var orderItemsProductIds= responseData.OrderItems.Select(o => o.ProductId);
        Assert.All(orderItemsProductIds, orderItemProdId => payloadItemsProductIds.Contains(orderItemProdId));
    }

    private CreateOrderDraftCommand FakeOrderDrafCommand()
    {
        return new CreateOrderDraftCommand(
            BuyerId: Guid.NewGuid().ToString(),
            new List<BasketItem>()
            {
                new BasketItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = 1,
                    ProductName = "Test",
                    UnitPrice = 10,
                    OldUnitPrice = 9,
                    Quantity = 1,
                    PictureUrl =Guid.NewGuid().ToString()
                }

            });
    }

    string BuildOrder()
    {
        var order = new
        {
            OrderNumber = "-1"
        };
        return JsonSerializer.Serialize(order);
    }
}

