using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ordering.Infrastructure;

namespace eShop.Ordering.API.Application.Queries
{
    public class OrderQueries(OrderingContext context)
        : IOrderQueries
    {
        public async Task<IEnumerable<Cardtype>> GetCardTypesAsync()=>
           await context.CardTypes.Select(c => new Cardtype { Id = c.Id, Name = c.Name }).ToListAsync();



        public async Task<Order> GetOrderAsync(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
                throw new KeyNotFoundException();

            return new Order
            {
                OrderNumber = order.Id,
                Date = order.OrderDate,
                Description = order.Description,
                City = order.Address.City,
                Country = order.Address.Country,
                State = order.Address.State,
                Street = order.Address.Street,
                Zipcode = order.Address.ZipCode,
                Status = order.OrderStatus.ToString(),
                Total = order.GetTotal(),
                OrderItems = order.OrderItems.Select(oi => new OrderItem
                {
                    ProductName = oi.ProductName,
                    Units = oi.Units,
                    UnitPrice = (double)oi.UnitPrice,
                    PictureUrl = oi.PictureUrl,
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
        {
            return await context.Orders
                .Where(o => o.Buyer.IdentityGuid == userId)
                .Select(o => new OrderSummary
                {
                    OrderNumber = o.Id,
                    Date = o.OrderDate,
                    Status = o.OrderStatus.ToString(),
                    Total = (double)o.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
                }).ToListAsync();
        }

      
    }
}
