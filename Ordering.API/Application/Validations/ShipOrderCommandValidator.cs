﻿using eShop.Ordering.API.Application.Commands;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Application.Validations
{
    public class ShipOrderCommandValidator:AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderCommandValidator(ILogger<ShipOrderCommandValidator> logger)
        {
            RuleFor(order => order.OrderNumber)
                .NotEmpty().WithMessage("Not orderId found");


            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("INSTANCE CREATED - {ClassName}", GetType().Name);
            }
        }
    }
}
