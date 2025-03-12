using System.Runtime.CompilerServices;
using Catalog.API.Apis;
using Catalog.API.Infrastructure;
using Catalog.API.IntegrationEvents;
using Catalog.API.Services;
using eShop.IntegrationEventLogEF.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using eShop.EventBusRabbitMQ;
using eShop.EventBus.Extension;
using Catalog.API.IntegrationEvents.Events;
using Catalog.API.IntegrationEvents.EventHandling;

namespace Catalog.API.Extension
{
    public static class Extensions
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {

            // Avoid loading full database config and migrations if startup
            // is being invoked from build-time OpenAPI generation
            if (builder.Environment.IsBuild())
            {
                builder.Services.AddDbContext<CatalogContext>();

                return;
            }

            builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", configureDbContextOptions: dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseNpgsql(builder =>
                {
                    builder.UseVector();
                });
            });

            // REVIEW: This is done for development ease but shouldn't be here in production
            builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

            //Add the integration services that consume the DbContext
            builder.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();

            builder.Services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

            builder.AddRabbitMqEventBus("eventbus")
                   .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
                   .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();


            builder.Services.AddOptions<CatalogOptions>()
                .BindConfiguration(nameof(CatalogOptions));

            if (builder.Configuration["OllamaEnabled"] is string ollamaEnabled && bool.Parse(ollamaEnabled))
            {
                builder.AddOllamaSharpEmbeddingGenerator("embedding");
                builder.Services.AddEmbeddingGenerator(b => b.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>())
                    .UseOpenTelemetry()
                    .UseLogging();
            }
            else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("openai")))
            {
                builder.AddOpenAIClientFromConfiguration("openai");
                builder.Services.AddEmbeddingGenerator(sp => sp.GetRequiredService<OpenAIClient>().AsEmbeddingGenerator(builder.Configuration["AI:OpenAI:EmbeddingModel"]!))
                    .UseOpenTelemetry()
                    .UseLogging()
                    .Build();
            }

            builder.Services.AddScoped<ICatalogAI, CatalogAI>();

        }
    }
}
