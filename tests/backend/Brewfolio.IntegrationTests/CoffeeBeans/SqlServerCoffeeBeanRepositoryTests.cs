using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Infrastructure.Persistence;
using Brewfolio.Infrastructure.Persistence.CoffeeBeans;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Brewfolio.IntegrationTests.CoffeeBeans;

public sealed class SqlServerCoffeeBeanRepositoryTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public SqlServerCoffeeBeanRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddedCoffeeBeanCanBeListedFromAFreshContext()
    {
        var options = new DbContextOptionsBuilder<BrewfolioDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        await using (var migrationContext = new BrewfolioDbContext(options))
        {
            await migrationContext.Database.MigrateAsync();
        }

        var createdAt = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        var name = $"Ethiopia Bombe {Guid.NewGuid():N}";
        var result = CoffeeBean.Create(
            new CoffeeBeanName(name),
            new RoasterName("Example Roasters"),
            createdAt);
        var coffeeBean = Assert.IsType<CoffeeBean>(result.Value);
        var bagResult = coffeeBean.AddBag(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 8, 28),
            null,
            250,
            18.50m,
            true,
            new DateOnly(2026, 9, 4),
            createdAt);
        Assert.IsType<Brewfolio.Domain.CoffeeBags.CoffeeBag>(bagResult.Value);

        await using (var writeContext = new BrewfolioDbContext(options))
        {
            var repository = new SqlServerCoffeeBeanRepository(writeContext);
            await repository.AddAsync(coffeeBean, CancellationToken.None);
        }

        await using (var readContext = new BrewfolioDbContext(options))
        {
            var repository = new SqlServerCoffeeBeanRepository(readContext);
            var stored = Assert.Single((await repository.QueryAsync(new CoffeeBeanQuery(name), CancellationToken.None)).Items);

            Assert.Equal(coffeeBean.Id, stored.Id);
            Assert.Equal(name, stored.Name.Value);
            Assert.Equal("Example Roasters", stored.Roaster.Value);
            Assert.Equal(createdAt, stored.CreatedAt);
            Assert.Single(stored.CoffeeBags);
        }
    }

    [Fact]
    public async Task CoffeeBagCanBeAddedToAnExistingTrackedCoffeeBean()
    {
        var options = new DbContextOptionsBuilder<BrewfolioDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        await using (var migrationContext = new BrewfolioDbContext(options))
        {
            await migrationContext.Database.MigrateAsync();
        }
        var bean = Assert.IsType<CoffeeBean>(CoffeeBean.Create(
            new CoffeeBeanName("Existing product"),
            new RoasterName("Example"),
            DateTimeOffset.UtcNow).Value);
        await using (var createContext = new BrewfolioDbContext(options))
        {
            await new SqlServerCoffeeBeanRepository(createContext).AddAsync(bean, CancellationToken.None);
        }

        await using var updateContext = new BrewfolioDbContext(options);
        var repository = new SqlServerCoffeeBeanRepository(updateContext);
        var tracked = await repository.GetAsync(bean.Id, CancellationToken.None);
        Assert.NotNull(tracked);
        var newBag = Assert.IsType<Brewfolio.Domain.CoffeeBags.CoffeeBag>(tracked.AddBag(
            new DateOnly(2026, 9, 1), null, null, 250, 10m, true,
            new DateOnly(2026, 9, 4), DateTimeOffset.UtcNow).Value);

        await repository.AddBagAsync(newBag, CancellationToken.None);
        Assert.Single((await repository.GetAsync(bean.Id, CancellationToken.None))!.CoffeeBags);
    }

    [Fact]
    public async Task QuerySearchesFiltersSortsAndContinuesWithAnOpaqueCursor()
    {
        var options = new DbContextOptionsBuilder<BrewfolioDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        await using var context = new BrewfolioDbContext(options);
        await context.Database.MigrateAsync();
        var repository = new SqlServerCoffeeBeanRepository(context);
        var prefix = $"Query-{Guid.NewGuid():N}";
        var now = new DateTimeOffset(2026, 9, 5, 10, 0, 0, TimeSpan.Zero);
        var first = CreateQueryBean($"{prefix}-A", new DateOnly(2026, 9, 1), false, now);
        var second = CreateQueryBean($"{prefix}-B", new DateOnly(2026, 9, 3), true, now.AddMinutes(1));
        var third = CreateQueryBean($"{prefix}-C", null, true, now.AddMinutes(2));
        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);
        await repository.AddAsync(third, CancellationToken.None);

        var inStock = await repository.QueryAsync(
            new CoffeeBeanQuery(prefix, true, null, CoffeeBeanSort.Name),
            CancellationToken.None);
        Assert.Equal(2, inStock.Items.Count);

        var pageOne = await repository.QueryAsync(
            new CoffeeBeanQuery(prefix, null, null, CoffeeBeanSort.LatestRoastDate, null, 2),
            CancellationToken.None);
        Assert.Equal([second.Id, first.Id], pageOne.Items.Select(bean => bean.Id));
        Assert.NotNull(pageOne.NextCursor);
        var pageTwo = await repository.QueryAsync(
            new CoffeeBeanQuery(prefix, null, null, CoffeeBeanSort.LatestRoastDate, pageOne.NextCursor, 2),
            CancellationToken.None);
        Assert.Equal(third.Id, Assert.Single(pageTwo.Items).Id);
    }

    private static CoffeeBean CreateQueryBean(
        string name,
        DateOnly? roastedOn,
        bool inStock,
        DateTimeOffset createdAt)
    {
        var bean = Assert.IsType<CoffeeBean>(CoffeeBean.Create(
            new CoffeeBeanName(name),
            new RoasterName("Query Roaster"),
            createdAt).Value);
        Assert.IsType<Brewfolio.Domain.CoffeeBags.CoffeeBag>(bean.AddBag(
            new DateOnly(2026, 9, 4), roastedOn, null, 250, 10m, inStock,
            new DateOnly(2026, 9, 5), createdAt).Value);
        return bean;
    }
}

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
