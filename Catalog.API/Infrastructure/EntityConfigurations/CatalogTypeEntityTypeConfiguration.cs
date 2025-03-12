using Catalog.API.Model;

namespace Catalog.API.Infrastructure.EntityConfigurations
{
    public class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType");

            builder.Property(cb => cb.Type)
                .HasMaxLength(100);
        }
    }
}
