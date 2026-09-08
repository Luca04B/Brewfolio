using Brewfolio.Domain.Results;
using Brewfolio.Domain.CoffeeBags;

namespace Brewfolio.Domain.CoffeeBeans;

public sealed class CoffeeBean
{
    private readonly List<CoffeeBag> _coffeeBags = [];
    private CoffeeBean()
    {
    }

    private CoffeeBean(
        CoffeeBeanId id,
        string name,
        string roaster,
        string? origin,
        RoastLevel roastLevel,
        string? description,
        string? productUrl,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Roaster = roaster;
        Origin = origin;
        RoastLevel = roastLevel;
        Description = description;
        ProductUrl = productUrl;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
 
    public CoffeeBeanId Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string Roaster { get; private set; } = null!;

    public string? Origin { get; private set; }

    public RoastLevel RoastLevel { get; private set; }

    public string? Description { get; private set; }

    public string? ProductUrl { get; private set; }

    public string? ImageKey { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<CoffeeBag> CoffeeBags => _coffeeBags;

    public Result<CoffeeBean> UpdateProfile(
        string name,
        string roaster,
        string? origin,
        RoastLevel roastLevel,
        string? description,
        string? productUrl,
        DateTimeOffset updatedAt)
    {
        var replacement = Create(name, roaster, origin, roastLevel, description, productUrl, updatedAt);
        if (replacement is Error error) return error;
        if (replacement is not CoffeeBean valid) throw new InvalidOperationException();
        Name = valid.Name;
        Roaster = valid.Roaster;
        Origin = valid.Origin;
        RoastLevel = valid.RoastLevel;
        Description = valid.Description;
        ProductUrl = valid.ProductUrl;
        UpdatedAt = updatedAt.ToUniversalTime();
        return this;
    }

    public void SetImage(string imageKey, DateTimeOffset updatedAt)
    {
        ImageKey = imageKey;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public string? RemoveImage(DateTimeOffset updatedAt)
    {
        var previousImageKey = ImageKey;
        ImageKey = null;
        UpdatedAt = updatedAt.ToUniversalTime();
        return previousImageKey;
    }

    public static Result<CoffeeBean> Create(string name, string roaster, DateTimeOffset createdAt)
    {
        return Create(name, roaster, null, RoastLevel.Unknown, null, null, createdAt);
    }

    public static Result<CoffeeBean> Create(
        string name,
        string roaster,
        string? origin,
        RoastLevel roastLevel,
        string? description,
        string? productUrl,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("coffeeBean.name.required", "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(roaster))
        {
            return Error.Validation("coffeeBean.roaster.required", "Roaster is required.");
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length > 120)
        {
            return Error.Validation("coffeeBean.name.tooLong", "Name cannot exceed 120 characters.");
        }

        var trimmedRoaster = roaster.Trim();
        if (trimmedRoaster.Length > 120)
        {
            return Error.Validation("coffeeBean.roaster.tooLong", "Roaster cannot exceed 120 characters.");
        }

        var trimmedOrigin = NormalizeOptional(origin);
        if (trimmedOrigin?.Length > 240)
        {
            return Error.Validation("coffeeBean.origin.tooLong", "Origin cannot exceed 240 characters.");
        }

        var trimmedDescription = NormalizeOptional(description);
        if (trimmedDescription?.Length > 1_000)
        {
            return Error.Validation(
                "coffeeBean.description.tooLong",
                "Description cannot exceed 1,000 characters.");
        }

        var trimmedProductUrl = NormalizeOptional(productUrl);
        if (trimmedProductUrl?.Length > 2_048)
        {
            return Error.Validation(
                "coffeeBean.productUrl.tooLong",
                "Product URL cannot exceed 2,048 characters.");
        }

        if (trimmedProductUrl is not null
            && (!Uri.TryCreate(trimmedProductUrl, UriKind.Absolute, out var parsedUrl)
                || (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps)))
        {
            return Error.Validation(
                "coffeeBean.productUrl.invalid",
                "Product URL must be a complete HTTP or HTTPS URL.");
        }

        return new CoffeeBean(
            Guid.NewGuid(),
            trimmedName,
            trimmedRoaster,
            trimmedOrigin,
            roastLevel,
            trimmedDescription,
            trimmedProductUrl,
            createdAt.ToUniversalTime());
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public Result<CoffeeBag> AddBag(
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        bool isInStock,
        DateOnly today,
        DateTimeOffset createdAt)
    {
        var result = CoffeeBag.Create(
            Id,
            purchasedOn,
            roastedOn,
            openedOn,
            initialWeightGrams,
            pricePaid,
            isInStock,
            today,
            createdAt);
        if (result is CoffeeBag coffeeBag)
        {
            _coffeeBags.Add(coffeeBag);
        }

        return result;
    }

    public Result<CoffeeBag> ReplaceBag(
        Guid coffeeBagId,
        DateOnly purchasedOn,
        DateOnly? roastedOn,
        DateOnly? openedOn,
        int initialWeightGrams,
        decimal pricePaid,
        bool isInStock,
        DateOnly today,
        DateTimeOffset updatedAt)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        return coffeeBag is null
            ? Error.NotFound("coffeeBag.notFound", "Coffee Bag was not found.")
            : coffeeBag.Replace(
                purchasedOn,
                roastedOn,
                openedOn,
                initialWeightGrams,
                pricePaid,
                isInStock,
                today,
                updatedAt);
    }

    public Result<CoffeeBag> MarkBagOpened(
        Guid coffeeBagId,
        DateOnly openedOn,
        DateTimeOffset updatedAt)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        return coffeeBag is null
            ? Error.NotFound("coffeeBag.notFound", "Coffee Bag was not found.")
            : coffeeBag.MarkOpened(openedOn, openedOn, updatedAt);
    }

    public Result<CoffeeBag> SetBagStock(
        Guid coffeeBagId,
        bool isInStock,
        DateTimeOffset updatedAt)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        return coffeeBag is null
            ? Error.NotFound("coffeeBag.notFound", "Coffee Bag was not found.")
            : coffeeBag.SetStock(isInStock, updatedAt);
    }

    public Result<CoffeeBag> RemoveBag(Guid coffeeBagId)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        if (coffeeBag is null)
        {
            return Error.NotFound("coffeeBag.notFound", "Coffee Bag was not found.");
        }

        _coffeeBags.Remove(coffeeBag);
        return coffeeBag;
    }
}
