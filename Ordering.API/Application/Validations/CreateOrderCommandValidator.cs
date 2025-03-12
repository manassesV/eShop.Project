using System;
using System.Collections.Generic;
using System.Linq;
using eShop.Ordering.API.Application.Commands;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Application.Validations
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {

        public CreateOrderCommandValidator(ILogger<CreateOrderCommandValidator> logger)
        {
            RuleFor(command => command.City).NotEmpty();
            RuleFor(command => command.Street).NotEmpty();
            RuleFor(command => command.State).NotEmpty();
            RuleFor(command => command.Country).NotEmpty();
            RuleFor(command => command.ZipCode).NotEmpty();
            RuleFor(command => command.CardNumber).NotEmpty().Length(12, 19);
            RuleFor(command => command.CardHolderName).NotEmpty();
            RuleFor(command => command.CardExpiration).NotEmpty()
                .Must(BeValidExpirationDate)
                .WithMessage("Please specify a valid card expiration date");
            RuleFor(command => command.CardSecurityNumber).NotEmpty().Length(3);
            RuleFor(command => command.CardTypeId).NotEmpty();
            RuleFor(command => command.OrderItems).Must(ContainsOrderItems)
                .WithMessage("No order items found");
        }

        private bool ContainsOrderItems(IEnumerable<OrderItemDTO> orderItems)
        {
            return orderItems.Any();
        }

        private bool BeValidExpirationDate(DateTime dateTime)
        {
            return dateTime >= DateTime.UtcNow;
        }
    }
}
