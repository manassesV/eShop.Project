﻿using System;
using System.Collections.Generic;
using System.Linq;
using eShop.Ordering.API.Application.Models;
using Ordering.API.Extensions;

namespace eShop.Ordering.API.Application.Commands;

[DataContract]
public class CreateOrderCommand:IRequest<bool>
{
    [DataMember]
    private readonly List<OrderItemDTO> _orderItems;
    [DataMember]
    public string UserId {  get; private set; }
    [DataMember]
    public string UserName { get; private set; }
    [DataMember]
    public string City { get; private set; }
    [DataMember]
    public string Street { get; private set; }
    [DataMember]
    public string State { get; private set; }
    [DataMember]
    public string Country { get; private set; }
    [DataMember]
    public string ZipCode {  get; private set; }
    [DataMember]
    public string CardNumber {  get; private set; }
    [DataMember]
    public string CardHolderName { get; private set; }
    [DataMember]
    public DateTime CardExpiration {  get; private set; }
    [DataMember]
    public string CardSecurityNumber {  get; private set; }
    [DataMember]
    public int CardTypeId { get; private set; }
    [DataMember]
    public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

    public CreateOrderCommand()
    {
        _orderItems = new List<OrderItemDTO>();
    }

    public CreateOrderCommand(
        List<BasketItem> basketItems,
        string userId,
        string userName,
        string city,
        string street,
        string state,
        string country,
        string zipcode,
        string cardNumber,
        string cardHolderName,
        DateTime cardExpiration,
        string cardSecurityNumber,
        int cardTypeId
        )
    {
        _orderItems = basketItems.ToOrderItemsDTO().ToList();
        UserId = userId;
        UserName = userName;
        City = city;
        Street = street;
        State = state;
        Country = country;
        ZipCode= zipcode;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
    }
}

public record OrderItemDTO
{
    public int ProductId { get; init; }
    public string ProductName { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public int Units { get; init; }
    public string PictureUrl { get; init; }
}