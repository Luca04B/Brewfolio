using Brewfolio.Domain.CoffeeBags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brewfolio.Infrastructure.Persistence.CoffeeBeans;

internal sealed class CoffeeBagConfiguration : IEntityTypeConfiguration<CoffeeBag>
{
    public void Configure(EntityTypeBuilder<CoffeeBag> builder)
    {
        builder.ToTable("CoffeeBags");
        builder.HasKey(coffeeBag => coffeeBag.Id);
        builder.Property(coffeeBag => coffeeBag.PricePaid).HasPrecision(10, 2);
        builder.Property(coffeeBag => coffeeBag.PurchasedOn).IsRequired();
        builder.Property(coffeeBag => coffeeBag.InitialWeightGrams).IsRequired();
        builder.Property(coffeeBag => coffeeBag.IsInStock).IsRequired();
        builder.HasIndex(coffeeBag => new { coffeeBag.CoffeeBeanId, coffeeBag.PurchasedOn });
    }
}
