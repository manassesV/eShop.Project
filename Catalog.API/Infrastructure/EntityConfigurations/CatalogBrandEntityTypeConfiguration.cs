using Catalog.API.Model;

namespace Catalog.API.Infrastructure.EntityConfigurations;

public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable(nameof(CatalogBrand));

        builder.Property(cb => cb.Brand)
            .HasMaxLength(100);
    }
}
