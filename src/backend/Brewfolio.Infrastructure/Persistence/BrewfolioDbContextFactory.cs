using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Brewfolio.Infrastructure.Persistence;

public sealed class BrewfolioDbContextFactory : IDesignTimeDbContextFactory<BrewfolioDbContext>
{
    public BrewfolioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BrewfolioDbContext>()
            .UseSqlServer("Server=localhost;Database=Brewfolio;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new BrewfolioDbContext(options);
    }
}
