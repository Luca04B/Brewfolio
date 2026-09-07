namespace Brewfolio.Application.CoffeeBeans;

public sealed record CoffeeBeanPageDto(IReadOnlyList<CoffeeBeanDto> Items, string? NextCursor);

public sealed class ListCoffeeBeansHandler(ICoffeeBeanRepository repository, TimeProvider timeProvider)
{
    public async Task<CoffeeBeanPageDto> HandleAsync(
        CoffeeBeanQuery query,
        CancellationToken cancellationToken)
    {
        var normalized = query with
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
        var page = await repository.QueryAsync(normalized, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        return new CoffeeBeanPageDto(
            page.Items.Select(bean => CoffeeBeanDto.From(bean, today)).ToArray(),
            page.NextCursor);
    }

    public Task<CoffeeBeanPageDto> HandleAsync(CancellationToken cancellationToken) =>
        HandleAsync(new CoffeeBeanQuery(), cancellationToken);
}
