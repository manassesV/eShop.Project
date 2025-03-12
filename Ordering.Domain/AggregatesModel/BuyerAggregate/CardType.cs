using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate;

public sealed class CardType
{
    public int Id { get; init; }
    public required string Name { get; init; }

}
