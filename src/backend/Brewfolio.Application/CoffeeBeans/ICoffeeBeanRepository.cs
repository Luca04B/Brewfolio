using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;

namespace Brewfolio.Application.CoffeeBeans;

public interface ICoffeeBeanRepository
{
    Task AddAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken);

    Task<CoffeeBeanQueryPage> QueryAsync(CoffeeBeanQuery query, CancellationToken cancellationToken);

    Task<CoffeeBean?> GetAsync(CoffeeBeanId id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task AddBagAsync(CoffeeBag coffeeBag, CancellationToken cancellationToken);

    Task DeleteAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken);

    Task<CoffeeBean?> FindExactAsync(
        CoffeeBeanName name,
        RoasterName roaster,
        CoffeeBeanId? excludingId,
        CancellationToken cancellationToken);
}

public enum CoffeeBeanSort { LatestActivity, Name, LatestRoastDate }

public sealed record CoffeeBeanQuery(
    string? Search = null,
    bool? IsInStock = null,
    RoastLevel? RoastLevel = null,
    CoffeeBeanSort Sort = CoffeeBeanSort.LatestActivity,
    string? Cursor = null,
    int PageSize = 24);

public sealed record CoffeeBeanQueryPage(IReadOnlyList<CoffeeBean> Items, string? NextCursor);

public sealed record StoredCoffeeBeanImage(Stream Content, string ContentType) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

public interface ICoffeeBeanImageStore
{
    Task<CoffeeBeanResult<string>> StoreAsync(
        CoffeeBeanId coffeeBeanId,
        byte[] content,
        CancellationToken cancellationToken);

    Task<StoredCoffeeBeanImage?> OpenAsync(string imageKey, bool thumbnail, CancellationToken cancellationToken);

    Task DeleteAsync(string imageKey, CancellationToken cancellationToken);
}

public interface IImageCleanupQueue
{
    Task EnqueueAsync(string imageKey, CancellationToken cancellationToken);
}
