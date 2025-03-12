using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Extensions;

public static class MigrateDbContextExtensions
{
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
   
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddMigration<TContext>((_, _) => Task.CompletedTask);

   
    public static IServiceCollection AddMigration<TContext>
        (this IServiceCollection services,
        Func<TContext, IServiceProvider, Task> seeder)
    where TContext : DbContext
    {
        //Enable migration tracing
        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
    }
    public static IServiceCollection AddMigration<TContext, IDbSeeder>(
        this IServiceCollection services) 
        where TContext : DbContext
        where IDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddScoped<IDbSeeder<TContext>, IDbSeeder>();
        return services.AddMigration<TContext>((context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
    }

    private static async Task MigrateDbContextAsync<TContext>
        (this IServiceProvider service, Func<TContext, IServiceProvider, Task> seeder)
        where TContext : DbContext
    {
        using var scope = service.CreateAsyncScope();
        var scopeService = scope.ServiceProvider;
        var logger = scopeService.GetRequiredService<ILogger<TContext>>();
        var context = scopeService.GetService<TContext>();

        using var activity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(()=> InvokeSeeder(seeder, context, scopeService));
        }
        catch (Exception)
        {

            throw;
        }

    }
    
    private static async Task InvokeSeeder<TContext>
   (Func<TContext, 
       IServiceProvider, Task> seeder, 
        TContext context, IServiceProvider service) where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            await context.Database.MigrateAsync();
            await seeder(context, service);
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);

            throw;
        }
    }


    private class MigrationHostedService<TContext>
        (IServiceProvider serviceProvider,
        Func<TContext, IServiceProvider, Task> seeder)
        : BackgroundService where TContext : DbContext
    {
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}



public interface IDbSeeder<in TContext> where TContext: DbContext
{
    Task SeedAsync(TContext context);
}