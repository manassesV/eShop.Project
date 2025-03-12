using Catalog.API.Infrastructure.EntityConfigurations;
using Catalog.API.Model;
using Microsoft.Extensions.Configuration;

namespace Catalog.API.Infrastructure
{
    public class CatalogContext:DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options,
            IConfiguration configuration)
        {
           
        }

        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<CatalogBrand> CatalogBrands { get; set; }
        public DbSet<CatalogType> CatalogTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");
            modelBuilder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());

            // Add the outbox table to this context
            //modelBuilder.UseIntegrationEventLogs();
        }
    }
}
