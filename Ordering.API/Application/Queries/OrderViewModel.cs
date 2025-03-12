using System;
using System.Collections.Generic;

namespace eShop.Ordering.API.Application.Queries
{
    public record OrderItem
    {
        public string ProductName { get; init; }
        public int Units { get; init; }
        public double UnitPrice { get; init; }
        public string PictureUrl { get; init; }
    }

    public record Order
    {
        public int OrderNumber { get; init; }
        public DateTime Date { get; init; }
        public string Status { get; init; }
        public string Description { get; init; }
        public string Street { get; init; }
        public string City { get; init; }
        public string State { get; init; }
        public string Zipcode { get; init; }
        public string Country { get; init; }
        public List<OrderItem> OrderItems { get; set; }

        public decimal Total { get; set; }
    }

    public record OrderSummary
    {
        public int OrderNumber { get; init; }
        public DateTime Date { get; init; }
        public string Status { get; init; }
        public double Total { get; init; }
    }
    public record Cardtype
    {
        public int Id { get; init; }
        public string Name { get; init; }
    }
}
