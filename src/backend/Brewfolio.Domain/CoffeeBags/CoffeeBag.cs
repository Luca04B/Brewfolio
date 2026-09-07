using Brewfolio.Domain.Results;
using Brewfolio.Domain.CoffeeBeans;

namespace Brewfolio.Domain.CoffeeBags;

public sealed class CoffeeBag
{
    private CoffeeBag()
    {
    }

    private CoffeeBag(
        CoffeeBagId id,
        CoffeeBeanId coffeeBeanId,
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        bool isInStock,
        DateTimeOffset createdAt)
    {
        Id = id;
        CoffeeBeanId = coffeeBeanId;
        PurchasedOn = purchasedOn;
        RoastedOn = roastedOn;
        OpenedOn = openedOn;
        InitialWeightGrams = initialWeightGrams;
        PricePaid = pricePaid;
        IsInStock = isInStock;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public CoffeeBagId Id { get; private set; } = null!;
    public CoffeeBeanId CoffeeBeanId { get; private set; } = null!;
    public DateOnly PurchasedOn { get; private set; }
    public DateOnly? RoastedOn { get; private set; }
    public DateOnly? OpenedOn { get; private set; }
    public int InitialWeightGrams { get; private set; }
    public decimal PricePaid { get; private set; }
    public bool IsInStock { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    internal static Result<CoffeeBag> Create(
        CoffeeBeanId coffeeBeanId,
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        bool isInStock,
        DateOnly today,
        DateTimeOffset createdAt)
    {
        var validationError = Validate(
            purchasedOn,
            roastedOn,
            openedOn,
            initialWeightGrams,
            pricePaid,
            today);
        if (validationError is not null)
        {
            return validationError;
        }

        return new CoffeeBag(
            new CoffeeBagId(Guid.NewGuid()),
            coffeeBeanId,
            purchasedOn,
            roastedOn,
            openedOn,
            initialWeightGrams,
            pricePaid,
            isInStock,
            createdAt.ToUniversalTime());
    }

    internal Result<CoffeeBag> Replace(
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        bool isInStock,
        DateOnly today,
        DateTimeOffset updatedAt)
    {
        var validationError = Validate(
            purchasedOn,
            roastedOn,
            openedOn,
            initialWeightGrams,
            pricePaid,
            today);
        if (validationError is not null)
        {
            return validationError;
        }

        PurchasedOn = purchasedOn;
        RoastedOn = roastedOn;
        OpenedOn = openedOn;
        InitialWeightGrams = initialWeightGrams;
        PricePaid = pricePaid;
        IsInStock = isInStock;
        UpdatedAt = updatedAt.ToUniversalTime();
        return this;
    }

    internal Result<CoffeeBag> MarkOpened(DateOnly openedOn, DateOnly today, DateTimeOffset updatedAt)
    {
        if (openedOn > today)
        {
            return Error.Validation("coffeeBag.date.future", "Coffee Bag dates cannot be in the future.");
        }

        if (RoastedOn.HasValue && openedOn < RoastedOn)
        {
            return Error.Validation(
                "coffeeBag.openedOn.beforeRoastedOn",
                "Opened date cannot precede the roast date.");
        }

        if (OpenedOn != openedOn)
        {
            OpenedOn = openedOn;
            UpdatedAt = updatedAt.ToUniversalTime();
        }

        return this;
    }

    internal CoffeeBag SetStock(bool isInStock, DateTimeOffset updatedAt)
    {
        if (IsInStock != isInStock)
        {
            IsInStock = isInStock;
            UpdatedAt = updatedAt.ToUniversalTime();
        }

        return this;
    }

    private static Error? Validate(
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        DateOnly today)
    {
        if (purchasedOn > today || roastedOn > today || openedOn > today)
        {
            return Error.Validation("coffeeBag.date.future", "Coffee Bag dates cannot be in the future.");
        }

        if (openedOn.HasValue && roastedOn.HasValue && openedOn < roastedOn)
        {
            return Error.Validation(
                "coffeeBag.openedOn.beforeRoastedOn",
                "Opened date cannot precede the roast date.");
        }

        if (initialWeightGrams <= 0)
        {
            return Error.Validation("coffeeBag.weight.invalid", "Initial weight must be positive.");
        }

        if (pricePaid < 0)
        {
            return Error.Validation("coffeeBag.price.negative", "Price paid cannot be negative.");
        }

        return null;
    }
}
