using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBeans;

public sealed class CoffeeBeanApplicationTests
{
    [Fact]
    public async Task CreatePersistsAndReturnsTheCoffeeBean()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        var repository = new RecordingCoffeeBeanRepository();
        var handler = new CreateCoffeeBeanHandler(repository, new FixedTimeProvider(now));

        var result = await handler.HandleAsync(
            new CreateCoffeeBeanCommand(
                "  Ethiopia Bombe  ",
                "  Example Roasters  ",
                "Ethiopia",
                RoastLevel.Light,
                "Floral",
                "https://example.com/coffee",
                new CoffeeBagInput(
                    new DateOnly(2026, 9, 1),
                    new DateOnly(2026, 8, 28),
                    null,
                    250,
                    18.50m,
                    true)),
            CancellationToken.None);

        var created = Assert.IsType<CoffeeBeanDto>(result.Value);
        Assert.Equal(repository.Added?.Id, created.Id);
        Assert.Equal("Ethiopia Bombe", created.Name);
        Assert.Equal("Example Roasters", created.Roaster);
        Assert.Equal(now, created.CreatedAt);
        Assert.Equal("Ethiopia", created.Origin);
        Assert.Equal(RoastLevel.Light, created.RoastLevel);
        Assert.Equal(1, created.BagCount);
        Assert.True(created.IsInStock);
        Assert.Equal(7.40m, created.WeightedPricePer100Grams);
    }

    [Fact]
    public async Task CreateReturnsValidationWithoutPersisting()
    {
        var repository = new RecordingCoffeeBeanRepository();
        var handler = new CreateCoffeeBeanHandler(repository, TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateCoffeeBeanCommand(
                "", "Example Roasters", null, RoastLevel.Unknown, null, null, null),
            CancellationToken.None);

        var failure = Assert.IsType<ValidationFailure>(result.Value);
        Assert.Equal(
            ValidationErrorCode.CoffeeBeanNameRequired,
            Assert.Single(failure.Errors).Code);
        Assert.Null(repository.Added);
    }

    [Fact]
    public async Task InvalidFirstBagLeavesNoCoffeeBeanPersisted()
    {
        var repository = new RecordingCoffeeBeanRepository();
        var handler = new CreateCoffeeBeanHandler(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync(
            new CreateCoffeeBeanCommand(
                "Bombe",
                "Example",
                null,
                RoastLevel.Unknown,
                null,
                null,
                new CoffeeBagInput(new DateOnly(2026, 9, 5), null, null, 250, 10m, true)),
            CancellationToken.None);

        Assert.Equal(
            ValidationErrorCode.CoffeeBagPurchasedDateInFuture,
            Assert.Single(Assert.IsType<ValidationFailure>(result.Value).Errors).Code);
        Assert.Null(repository.Added);
    }

    [Fact]
    public async Task ListReturnsStoredCoffeeBeansNewestFirst()
    {
        var older = CreateBean("Older", new DateTimeOffset(2026, 9, 3, 10, 0, 0, TimeSpan.Zero));
        var newer = CreateBean("Newer", new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero));
        var repository = new RecordingCoffeeBeanRepository(older, newer);
        var handler = new ListCoffeeBeansHandler(repository, TimeProvider.System);

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.Collection(
            result.Items,
            bean => Assert.Equal("Newer", bean.Name),
            bean => Assert.Equal("Older", bean.Name));
    }

    [Fact]
    public async Task DetailReturnsNotFoundForAnUnknownCoffeeBean()
    {
        var handler = new GetCoffeeBeanHandler(new RecordingCoffeeBeanRepository(), TimeProvider.System);

        var result = await handler.HandleAsync(
            new CoffeeBeanId(Guid.NewGuid()),
            CancellationToken.None);

        var error = Assert.IsType<CoffeeBeanError>(result.Value);
        Assert.Equal(CoffeeBeanErrorType.NotFound, error.Type);
        Assert.Equal(CoffeeBeanErrorCode.CoffeeBeanNotFound, error.Code);
    }

    [Fact]
    public async Task AddingAndTogglingABagRefreshesTheProductSummary()
    {
        var coffeeBean = CreateBean("Bombe", new DateTimeOffset(2026, 9, 4, 8, 0, 0, TimeSpan.Zero));
        var repository = new RecordingCoffeeBeanRepository(coffeeBean);
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero));
        var create = new CreateCoffeeBagHandler(repository, time);

        var created = await create.HandleAsync(
            coffeeBean.Id,
            new CoffeeBagInput(new DateOnly(2026, 9, 4), null, null, 250, 10m, true),
            CancellationToken.None);
        var withBag = Assert.IsType<CoffeeBeanDto>(created.Value);
        var toggle = new SetCoffeeBagStockHandler(repository, time);
        var toggled = await toggle.HandleAsync(
            coffeeBean.Id,
            new CoffeeBagId(withBag.Bags[0].Id),
            false,
            CancellationToken.None);

        Assert.False(Assert.IsType<CoffeeBeanDto>(toggled.Value).IsInStock);
        Assert.Equal(2, repository.SaveCount);
    }

    private static CoffeeBean CreateBean(string name, DateTimeOffset createdAt)
    {
        var result = CoffeeBean.Create(
            new CoffeeBeanName(name),
            new RoasterName("Example Roasters"),
            createdAt);
        return Assert.IsType<CoffeeBean>(result.Value);
    }

    private sealed class RecordingCoffeeBeanRepository(params CoffeeBean[] coffeeBeans)
        : ICoffeeBeanRepository
    {
        private readonly List<CoffeeBean> _coffeeBeans = [.. coffeeBeans];

        public CoffeeBeanDto? Added { get; private set; }
        public int SaveCount { get; private set; }

        public Task AddAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken)
        {
            _coffeeBeans.Add(coffeeBean);
            Added = CoffeeBeanDto.From(coffeeBean, new DateOnly(2026, 9, 4));
            return Task.CompletedTask;
        }

        public Task<CoffeeBeanQueryPage> QueryAsync(
            CoffeeBeanQuery query,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new CoffeeBeanQueryPage(
                _coffeeBeans.OrderByDescending(bean => bean.CreatedAt).ToArray(),
                null));
        }

        public Task<CoffeeBean?> GetAsync(CoffeeBeanId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_coffeeBeans.SingleOrDefault(coffeeBean => coffeeBean.Id == id));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task AddBagAsync(CoffeeBag coffeeBag, CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken)
        {
            _coffeeBeans.Remove(coffeeBean);
            return Task.CompletedTask;
        }

        public Task<CoffeeBean?> FindExactAsync(
            CoffeeBeanName name,
            RoasterName roaster,
            CoffeeBeanId? excludingId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_coffeeBeans.FirstOrDefault(bean =>
                bean.Id != excludingId
                && string.Equals(bean.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase)
                && string.Equals(bean.Roaster.Value, roaster.Value, StringComparison.OrdinalIgnoreCase)));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
