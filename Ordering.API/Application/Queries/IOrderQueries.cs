using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Application.Queries
{
    public interface IOrderQueries
    {
        Task<Order> GetOrderAsync(int id);
        Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId);

        Task<IEnumerable<Cardtype>> GetCardTypesAsync();
    }
}
