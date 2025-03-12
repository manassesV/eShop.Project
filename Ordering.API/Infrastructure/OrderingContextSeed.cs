using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eShop.Ordering.API.Extensions;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Infrastructure;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class OrderingContextSeed : IDbSeeder<OrderingContext>
    {
        public async Task SeedAsync(OrderingContext context)
        {

            if (!context.CardTypes.Any())
            {

                context.CardTypes.AddRange(GetPredefinedCardTypes());

                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }

        private static IEnumerable<CardType> GetPredefinedCardTypes()
        {
            yield return new CardType { Id = 1, Name ="Amex" };
            yield return new CardType { Id = 2, Name = "Amex" };
            yield return new CardType { Id = 3, Name ="Amex" };
        }
    }
}
