using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBeans;

public sealed class CoffeeBeanImageLifecycleTests
{
    [Fact]
    public async Task ProductDeletionSucceedsAndQueuesCleanupWhenStorageIsUnavailable()
    {
        var bean = CreateBean();
        bean.SetImage("old-key", DateTimeOffset.UtcNow);
        var repository = new Repository(bean);
        var queue = new CleanupQueue();
        var handler = new DeleteCoffeeBeanHandler(repository, new FailingImageStore(), queue);

        var result = await handler.HandleAsync(bean.Id, CancellationToken.None);

        Assert.True(Assert.IsType<bool>(result.Value));
        Assert.True(repository.Deleted);
        Assert.Equal("old-key", Assert.Single(queue.Keys));
    }

    [Fact]
    public async Task FailedReplacementKeepsExistingImageAndProductData()
    {
        var bean = CreateBean();
        bean.SetImage("old-key", DateTimeOffset.UtcNow);
        var repository = new Repository(bean);
        var handler = new ReplaceCoffeeBeanImageHandler(
            repository,
            new FailingImageStore(),
            new CleanupQueue(),
            TimeProvider.System);

        var result = await handler.HandleAsync(bean.Id, [1, 2, 3], CancellationToken.None);

        Assert.Equal(
            CoffeeBeanErrorCode.ImageStorageUnavailable,
            Assert.IsType<CoffeeBeanError>(result.Value).Code);
        Assert.Equal("old-key", bean.ImageKey);
        Assert.Equal(0, repository.SaveCount);
    }

    private static CoffeeBean CreateBean() => Assert.IsType<CoffeeBean>(
        CoffeeBean.Create(
            new CoffeeBeanName("Image product"),
            new RoasterName("Roaster"),
            DateTimeOffset.UtcNow).Value);

    private sealed class FailingImageStore : ICoffeeBeanImageStore
    {
        public Task<CoffeeBeanResult<string>> StoreAsync(
            CoffeeBeanId coffeeBeanId,
            byte[] content,
            CancellationToken cancellationToken) =>
            Task.FromResult<CoffeeBeanResult<string>>(
                new CoffeeBeanError(CoffeeBeanErrorCode.ImageStorageUnavailable));
        public Task<StoredCoffeeBeanImage?> OpenAsync(string imageKey, bool thumbnail, CancellationToken cancellationToken) =>
            Task.FromResult<StoredCoffeeBeanImage?>(null);
        public Task DeleteAsync(string imageKey, CancellationToken cancellationToken) =>
            Task.FromException(new InvalidOperationException("Unavailable"));
    }

    private sealed class CleanupQueue : IImageCleanupQueue
    {
        public List<string> Keys { get; } = [];
        public Task EnqueueAsync(string imageKey, CancellationToken cancellationToken)
        {
            Keys.Add(imageKey);
            return Task.CompletedTask;
        }
    }

    private sealed class Repository(CoffeeBean bean) : ICoffeeBeanRepository
    {
        public bool Deleted { get; private set; }
        public int SaveCount { get; private set; }
        public Task AddAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<CoffeeBeanQueryPage> QueryAsync(CoffeeBeanQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new CoffeeBeanQueryPage([bean], null));
        public Task<CoffeeBean?> GetAsync(CoffeeBeanId id, CancellationToken cancellationToken) =>
            Task.FromResult(id == bean.Id ? bean : null);
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
        public Task AddBagAsync(CoffeeBag coffeeBag, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken)
        {
            Deleted = true;
            return Task.CompletedTask;
        }
        public Task<CoffeeBean?> FindExactAsync(
            CoffeeBeanName name,
            RoasterName roaster,
            CoffeeBeanId? excludingId,
            CancellationToken cancellationToken) =>
            Task.FromResult<CoffeeBean?>(null);
    }
}
