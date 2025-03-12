using eShop.Ordering.API.Application.Queries;
using Microsoft.Extensions.Logging;
using Ordering.API.Infrastructure.Services;

namespace eShop.Ordering.API.Apis
{
    public class OrderServices(
        IMediator mediator,
        IOrderQueries queries,
        IIdentityService identityService,
        ILogger<OrderServices> logger
        )
    {
        public IMediator Mediator { get; set; } = mediator;
        public ILogger<OrderServices> Logger { get; set; } = logger;
        public IOrderQueries Queries { get; } = queries;
        public IIdentityService IdentityService { get; } = identityService;

    }
}
