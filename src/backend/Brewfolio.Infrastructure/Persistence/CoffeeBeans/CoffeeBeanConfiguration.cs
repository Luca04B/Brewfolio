using Brewfolio.Domain.CoffeeBeans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brewfolio.Infrastructure.Persistence.CoffeeBeans;

internal sealed class CoffeeBeanConfiguration : IEntityTypeConfiguration<CoffeeBean>
{
    public void Configure(EntityTypeBuilder<CoffeeBean> builder)
    {
        builder.ToTable("CoffeeBeans");
        builder.HasKey(coffeeBean => coffeeBean.Id);

        builder.Property(coffeeBean => coffeeBean.Name)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(coffeeBean => coffeeBean.Roaster)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(coffeeBean => coffeeBean.Origin).HasMaxLength(240);
        builder.Property(coffeeBean => coffeeBean.RoastLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(coffeeBean => coffeeBean.Description).HasMaxLength(1_000);
        builder.Property(coffeeBean => coffeeBean.ProductUrl).HasMaxLength(2_048);
        builder.Property(coffeeBean => coffeeBean.ImageKey).HasMaxLength(320);
        builder.Property(coffeeBean => coffeeBean.CreatedAt).IsRequired();
        builder.Property(coffeeBean => coffeeBean.UpdatedAt).IsRequired();

        builder.HasIndex(coffeeBean => coffeeBean.CreatedAt);
        builder.HasIndex(coffeeBean => new { coffeeBean.Roaster, coffeeBean.Name });

        builder.HasMany(coffeeBean => coffeeBean.CoffeeBags)
            .WithOne()
            .HasForeignKey(coffeeBag => coffeeBag.CoffeeBeanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(coffeeBean => coffeeBean.CoffeeBags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
