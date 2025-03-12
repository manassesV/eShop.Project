using eShop.EventBus.Extension;
using eShop.EventBusRabbitMQ;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.API.Application.Behaviors;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.Validations;
using eShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ordering.API.Infrastructure.Services;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Infrastructure;

namespace eShop.Ordering.API.Extensions
{
    public static class Extensions
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            //Add the authentication services to DI
            builder.AddDefaultAuthentication();


            // Pooling is disabled because of the following error:
            // Unhandled exception. System.InvalidOperationException:
            // The DbContext of type 'OrderingContext' cannot be pooled because it does not have a public constructor accepting a single parameter of type DbContextOptions or has more than one constructor.
            services.AddDbContext<OrderingContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("orderingdb"));
            });

            builder.EnrichNpgsqlDbContext<OrderingContext>();

            services.AddMigration<OrderingContext, OrderingContextSeed>();


            //Add the integration services that consume the DBContext
            services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<OrderingContext>>();

            services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();

            //builder.AddRabbitMqEventBus("eventbus")
                //.AddSubscription();

            services.AddHttpContextAccessor();
            services.AddTransient<IIdentityService, IdentityService>();

            //Consfigure mediaR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining(typeof(Program));

                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });

            // Register the command validators for the validator behavior (validators based on FluentValidation library)
            services.AddSingleton<IValidator<CancelOrderCommand>, CancelOrderCommandValidator>();
        }
    }
}
