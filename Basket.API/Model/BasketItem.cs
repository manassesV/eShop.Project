﻿using System.ComponentModel.DataAnnotations;

namespace Basket.API.Model
{
    public class BasketItem : IValidatableObject
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OldUnitPrice { get; set; }
        public int Quantity { get; set; }
        public string PictureUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new  List<ValidationResult>();

            if(Quantity < 1)
            {
                results.Add(new ValidationResult("Invalid numbeer of units", new[] { "Quantity" }));
            }

            return results;
        }
    }
}
