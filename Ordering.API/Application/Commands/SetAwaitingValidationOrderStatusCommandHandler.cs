using System.Threading;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using Microsoft.Extensions.Logging;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Infrastructure.Idempotency;

namespace eShop.Ordering.API.Application.Commands;

// Regular CommandHandler
public class SetAwaitingValidationOrderStatusCommandHandler : IRequestHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public SetAwaitingValidationOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    /// <summary>
    /// Handler which processes the command when
    /// graceperiod has finished
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public async Task<bool> Handle(SetAwaitingValidationOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);

        if (orderToUpdate == null)
        {
            return false;
        }
        orderToUpdate.SetAwaitingValidationStatus();
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}

//Use for Idempotency in command process
public class SetAwaitingValidationIdentifiedOrderStatusCommandHandler : IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    public SetAwaitingValidationIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>> logger)
        :base(mediator, requestManager, logger)
    {
            
    }
    protected override bool CreateResultForDuplicateRequest()
    {
        return true;// Ignore duplicate request for processing order.
    }
}