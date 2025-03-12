using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregatesModel.BuyerAggregate;

namespace Ordering.Infrastructure.EntityConfigurations
{
    class CardTypeEntityTypeConfiguration : IEntityTypeConfiguration<CardType>
    {
        public void Configure(EntityTypeBuilder<CardType> cardTypesConfiguration)
        {
            cardTypesConfiguration.ToTable("cardtypes");

            cardTypesConfiguration.Property(ct => ct.Id)
                .ValueGeneratedNever();

            cardTypesConfiguration.Property<string>(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
