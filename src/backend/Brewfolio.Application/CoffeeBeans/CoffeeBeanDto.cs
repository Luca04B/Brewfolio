using Brewfolio.Domain.CoffeeBeans;

namespace Brewfolio.Application.CoffeeBeans;

public sealed record CoffeeBeanDto(
    Guid Id,
    string Name,
    string Roaster,
    string? Origin,
    RoastLevel RoastLevel,
    string? Description,
    string? ProductUrl,
    string? ImageUrl,
    string? ImageThumbnailUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CoffeeBagDto> Bags,
    int BagCount,
    bool IsInStock,
    DateOnly? LatestRoastedOn,
    int? LatestRoastAgeDays,
    DateOnly? LatestOpenedOn,
    int? LatestOpenAgeDays,
    decimal? WeightedPricePer100Grams,
    DateTimeOffset LatestActivityAt)
{
    public static CoffeeBeanDto From(CoffeeBean coffeeBean, DateOnly today)
    {
        var bags = coffeeBean.CoffeeBags
            .OrderByDescending(coffeeBag => coffeeBag.PurchasedOn)
            .ThenByDescending(coffeeBag => coffeeBag.CreatedAt)
            .Select(CoffeeBagDto.From)
            .ToArray();
        var latestRoastedOn = coffeeBean.CoffeeBags.Max(coffeeBag => coffeeBag.RoastedOn);
        var latestOpenedOn = coffeeBean.CoffeeBags.Max(coffeeBag => coffeeBag.OpenedOn);
        var totalWeight = coffeeBean.CoffeeBags.Sum(coffeeBag => coffeeBag.InitialWeightGrams);
        var weightedPrice = totalWeight == 0
            ? (decimal?)null
            : decimal.Round(
                coffeeBean.CoffeeBags.Sum(coffeeBag => coffeeBag.PricePaid) / totalWeight * 100m,
                2,
                MidpointRounding.AwayFromZero);

        return new CoffeeBeanDto(
            coffeeBean.Id.Value,
            coffeeBean.Name.Value,
            coffeeBean.Roaster.Value,
            coffeeBean.Origin,
            coffeeBean.RoastLevel,
            coffeeBean.Description,
            coffeeBean.ProductUrl,
            coffeeBean.ImageKey is null ? null : $"/api/coffee-beans/{coffeeBean.Id}/image/large",
            coffeeBean.ImageKey is null ? null : $"/api/coffee-beans/{coffeeBean.Id}/image/thumbnail",
            coffeeBean.CreatedAt,
            coffeeBean.UpdatedAt,
            bags,
            bags.Length,
            coffeeBean.CoffeeBags.Any(coffeeBag => coffeeBag.IsInStock),
            latestRoastedOn,
            latestRoastedOn.HasValue ? today.DayNumber - latestRoastedOn.Value.DayNumber : null,
            latestOpenedOn,
            latestOpenedOn.HasValue ? today.DayNumber - latestOpenedOn.Value.DayNumber : null,
            weightedPrice,
            coffeeBean.CoffeeBags
                .Select(coffeeBag => coffeeBag.UpdatedAt)
                .Append(coffeeBean.UpdatedAt)
                .Max());
    }
}
