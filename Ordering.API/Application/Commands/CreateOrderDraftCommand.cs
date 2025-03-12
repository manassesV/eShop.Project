using System.Collections.Generic;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.Application.Commands
{
    public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items)
        :IRequest<OrderDraftDTO>;
    
}
