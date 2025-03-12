using System.Threading.Tasks;
using Ordering.Domain.SeedWork;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public interface IBuyerRepository:IRepository<Buyer>
    {
        Buyer Add(Buyer buyer);
        Buyer Update(Buyer buyer);
        Task<Buyer> FindAsync(string BuyerIdentifyGuid);
        Task<Buyer> FindByIdAsync(int Id);
    }
}
