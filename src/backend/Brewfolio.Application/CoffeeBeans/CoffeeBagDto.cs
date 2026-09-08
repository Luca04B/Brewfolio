using Brewfolio.Domain.CoffeeBags;

namespace Brewfolio.Application.CoffeeBeans;

public sealed record CoffeeBagDto(
    Guid Id,
    DateOnly PurchasedOn,
    DateOnly? RoastedOn,
    DateOnly? OpenedOn,
    int InitialWeightGrams,
    decimal PricePaid,
    bool IsInStock,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static CoffeeBagDto From(CoffeeBag coffeeBag)
    {
        return new CoffeeBagDto(
            coffeeBag.Id.Value,
            coffeeBag.PurchasedOn,
            coffeeBag.RoastedOn,
            coffeeBag.OpenedOn,
            coffeeBag.InitialWeightGrams,
            coffeeBag.PricePaid,
            coffeeBag.IsInStock,
            coffeeBag.CreatedAt,
            coffeeBag.UpdatedAt);
    }
}

public sealed record CoffeeBagInput(
    DateOnly PurchasedOn,
    DateOnly? RoastedOn,
    DateOnly? OpenedOn,
    int InitialWeightGrams,
    decimal PricePaid,
    bool IsInStock = true);
