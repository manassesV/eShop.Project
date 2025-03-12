using eShop.Ordering.API.Application.Commands;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Application.Validations
{
    public class IdentifiedCommandValidator: AbstractValidator<IdentifiedCommand<CreateOrderCommand, bool>>
    {
        public IdentifiedCommandValidator(ILogger<IdentifiedCommandValidator> logger)
        {
            RuleFor(command => command.Id).NotEmpty();

            if(logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("INSTANCE CREATED - {ClassName}", GetType().Name);
        }
    }
}
