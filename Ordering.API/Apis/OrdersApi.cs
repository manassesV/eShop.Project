using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using eShop.EventBus.Extension;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Apis;

public static class OrdersApi
{
    public static RouteGroupBuilder MapOrderApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/orders").HasApiVersion(1.0);

        api.MapPut("/cancel", CancelOrderAsync);
   
        api.MapPut("/ship", ShipOrderAsync);
        api.MapGet("{orderId:int}", GetOrderAsync);
        api.MapGet("/", GetOrdersAsync);
        api.MapGet("/cardtypes", GetCardTypesAsync);
        api.MapPost("/draft", CreateOrderDraftAsync).
            AddEndpointFilter<ValidationFilter<Order>>().Accepts<Order>("text/plain");
        api.MapPost("/", CreateOrderAsync);
        api.MapFallback(() => Results.Json("Route not found"));

        return api;
    }

    private static async Task<Results<Ok<Order>, NotFound>> GetOrdersAsync(
        int orderId,
        [AsParameters] OrderServices orderServices
        ) 
    {
        try
        {
           var order = await orderServices.Queries.GetOrderAsync(orderId);

            return TypedResults.Ok(order);
        }
        catch (Exception)
        {
           return  TypedResults.NotFound();
        }
    }

    public static async Task<Ok<IEnumerable<Cardtype>>> GetCardTypesAsync(
        IOrderQueries orderQueries)
    {
        var cardTypes = await orderQueries.GetCardTypesAsync();
        return TypedResults.Ok(cardTypes);
    }

    public static async Task<Ok<IEnumerable<OrderSummary>>> GetOrdersByUserAsync(
        [AsParameters] OrderServices orderServices
        )
    {
        var userId = orderServices.IdentityService.GetUserIdentity();
        var orders = await orderServices.Queries.GetOrdersFromUserAsync(userId);

        return TypedResults.Ok(orders);
    }

    public static async Task<Results<Ok<Order>, NotFound>> GetOrderAsync(
        int orderId,
        [AsParameters] OrderServices orderServices
        )
    {
        try
        {
            var order = await orderServices.Queries.GetOrderAsync(orderId);
            return TypedResults.Ok(order);
        }
        catch (Exception)
        {
            return TypedResults.NotFound();
        }
    }

    public static async Task<OrderDraftDTO> CreateOrderDraftAsync(
        CreateOrderDraftCommand command,
        [AsParameters] OrderServices services)
    {
        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty} {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.BuyerId),
            command.BuyerId,
            command);

        return await services.Mediator.Send(command);
    }

    public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestid,
        CreateOrderRequest request,
        [AsParameters] OrderServices services
        )
    {
        //mask the credit card number
        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty} : {CommandId}",
            request.GetGenericTypeName(),
            nameof(request.UserId),
            request.UserId);

        if (requestid == Guid.Empty)
        {
            services.Logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request);

            return TypedResults.BadRequest("RequestId is missing.");
        }

        using(services.Logger.BeginScope(new List<KeyValuePair<string, object>> { new("IdentifiedCommandId", requestid) }))
        {
            var maskedCCNumber = request.CardNumber.Substring(request.CardNumber.Length - 4)
                .PadLeft(request.CardNumber.Length, 'X');
            var createOrderCommand = new CreateOrderCommand(request.Items, request.UserId,
                request.UserName, request.City, request.Street,
                request.State, request.Country, request.ZipCode,
                maskedCCNumber, request.CardHolderName, request.CardExpiration,
                request.CardSecurityNumber, request.CardTypeId);

            var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, requestid);

            services.Logger.LogInformation(
                "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                requestCreateOrder.GetGenericTypeName(),
                nameof(requestCreateOrder.Id),
                requestCreateOrder.Id,
                requestCreateOrder);

            var result = await services.Mediator.Send(requestCreateOrder);

            if(result)
            {
                services.Logger.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestid);

            }
            else
            {
                services.Logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestid);

            }

            return TypedResults.Ok();
        }
    }

    public static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> ShipOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestid,
        ShipOrderCommand command,
        [AsParameters] OrderServices services)
    {
        if (requestid == Guid.Empty)
            return TypedResults.BadRequest("Empty GUID is not valid for request ID");

        var requestShipOrder = new IdentifiedCommand<ShipOrderCommand, bool>(command, requestid);

        services.Logger.LogError(
          "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
           requestShipOrder.GetGenericTypeName(),
           nameof(requestShipOrder.Command.OrderNumber),
           requestShipOrder.Command.OrderNumber,
           requestShipOrder);

        var commandResult = await services.Mediator.Send(requestShipOrder);

        if (!commandResult)
            return TypedResults.Problem(detail: "Ship order failed to process.", statusCode: 500);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CancelOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestid,
        CancelOrderCommand command,
        [AsParameters] OrderServices services)
    {
        if (requestid == Guid.Empty)
            return TypedResults.BadRequest("Empty GUID is not valid for request ID");


        var requestCancelOrder = new IdentifiedCommand<CancelOrderCommand, bool>(command, requestid);

        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            requestCancelOrder.GetGenericTypeName(),
            nameof(requestCancelOrder.Command.OrderNumber),
            requestCancelOrder.Command.OrderNumber,
            requestCancelOrder);

        var commandResult = await services.Mediator.Send(requestCancelOrder);

        if (!commandResult)
        {
            return TypedResults.Problem(detail: "Cancel order failed to process.", statusCode: 500);
        }

        return TypedResults.Ok();
    }

   
}

public record CreateOrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);


public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if(context.Arguments.FirstOrDefault(arg => arg is T) is not T arguments)
        {
            return Results.BadRequest("Invalid input");
        }

        return await next(context);
    }
}