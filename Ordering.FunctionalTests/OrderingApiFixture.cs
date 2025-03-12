

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Ordering.FunctionalTests
{
    public sealed class OrderingApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly IHost _app;
        public IResourceBuilder<PostgresServerResource> Postgress { get; private set; }
        public IResourceBuilder<PostgresServerResource> IdentityDB { get; private set; }
        public IResourceBuilder<ProjectResource> IdentityApi { get; private set; }

        private string _postgresConnectionString;

        public OrderingApiFixture()
        {
            var optios = new DistributedApplicationOptions
            {
                AssemblyName = typeof(OrderingApiFixture).Assembly.FullName,
                DisableDashboard = true
            };

            var appBuilder = DistributedApplication.CreateBuilder(optios);
            Postgress = appBuilder.AddPostgres("OrderingDB");
            IdentityDB = appBuilder.AddPostgres("IdentityDB");
            //IdentityApi = appBuilder.AddProject<Identity_API>("identity-api").WithReference(IdentityDB);
            _app = appBuilder.Build();
                
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"ConnectionStrings:{Postgress.Resource.Name}", _postgresConnectionString },
                    { "Identity:Url", IdentityApi.GetEndpoint("http").Url }
                });


            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
            });

            return base.CreateHost(builder);
        }
        public async Task InitializeAsync()
        {
            await _app.StartAsync();
            _postgresConnectionString = await Postgress.Resource.GetConnectionStringAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await base.DisposeAsync();
            await _app.StopAsync();

            if(_app is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else{
                _app.Dispose();
            }
        }

        private class AutoAuthorizeStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return builder =>
                {
                    builder.UseMiddleware<AutoAuthorizeMiddleware>();
                    next(builder);
                };
            }
        }
    }

    
}
