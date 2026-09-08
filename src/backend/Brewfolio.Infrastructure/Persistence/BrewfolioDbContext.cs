using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.CoffeeBags;
using Microsoft.EntityFrameworkCore;
using Brewfolio.Infrastructure.Images;
using StrongOf.EntityFrameworkCore;

namespace Brewfolio.Infrastructure.Persistence;

public sealed class BrewfolioDbContext(DbContextOptions<BrewfolioDbContext> options) : DbContext(options)
{
    public DbSet<CoffeeBean> CoffeeBeans => Set<CoffeeBean>();
    public DbSet<CoffeeBag> CoffeeBags => Set<CoffeeBag>();
    public DbSet<ImageCleanupJob> ImageCleanupJobs => Set<ImageCleanupJob>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterStrongOf<CoffeeBeanId, Guid>();
        configurationBuilder.RegisterStrongOf<CoffeeBeanName, string>();
        configurationBuilder.RegisterStrongOf<RoasterName, string>();
        configurationBuilder.RegisterStrongOf<CoffeeBagId, Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrewfolioDbContext).Assembly);
    }
}
