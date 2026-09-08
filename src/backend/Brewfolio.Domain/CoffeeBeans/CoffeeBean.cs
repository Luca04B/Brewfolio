using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.CoffeeBeans;

public sealed class CoffeeBean
{
    private readonly List<CoffeeBag> _coffeeBags = [];

    private CoffeeBean()
    {
    }

    private CoffeeBean(
        CoffeeBeanId id,
        CoffeeBeanName name,
        RoasterName roaster,
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

    public CoffeeBeanId Id { get; private set; } = null!;

    public CoffeeBeanName Name { get; private set; } = null!;

    public RoasterName Roaster { get; private set; } = null!;

    public string? Origin { get; private set; }

    public RoastLevel RoastLevel { get; private set; }

    public string? Description { get; private set; }

    public string? ProductUrl { get; private set; }

    public string? ImageKey { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<CoffeeBag> CoffeeBags => _coffeeBags;

    public Result<CoffeeBean> UpdateProfile(
        CoffeeBeanName name,
        RoasterName roaster,
        string? origin,
        RoastLevel roastLevel,
        string? description,
        string? productUrl,
        DateTimeOffset updatedAt)
    {
        var replacement = Create(name, roaster, origin, roastLevel, description, productUrl, updatedAt);
        if (replacement is ValidationFailure failure) return failure;
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

    public static Result<CoffeeBean> Create(
        CoffeeBeanName name,
        RoasterName roaster,
        DateTimeOffset createdAt)
    {
        return Create(name, roaster, null, RoastLevel.Unknown, null, null, createdAt);
    }

    public static Result<CoffeeBean> Create(
        CoffeeBeanName name,
        RoasterName roaster,
        string? origin,
        RoastLevel roastLevel,
        string? description,
        string? productUrl,
        DateTimeOffset createdAt)
    {
        var trimmedOrigin = NormalizeOptional(origin);
        if (trimmedOrigin?.Length > 240)
        {
            return ValidationFailure(
                ValidationErrorCode.CoffeeBeanOriginTooLong);
        }

        var trimmedDescription = NormalizeOptional(description);
        if (trimmedDescription?.Length > 1_000)
        {
            return ValidationFailure(
                ValidationErrorCode.CoffeeBeanDescriptionTooLong);
        }

        var trimmedProductUrl = NormalizeOptional(productUrl);
        if (trimmedProductUrl?.Length > 2_048)
        {
            return ValidationFailure(
                ValidationErrorCode.CoffeeBeanProductUrlTooLong);
        }

        if (trimmedProductUrl is not null
            && (!Uri.TryCreate(trimmedProductUrl, UriKind.Absolute, out var parsedUrl)
                || (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps)))
        {
            return ValidationFailure(
                ValidationErrorCode.CoffeeBeanProductUrlInvalid);
        }

        return new CoffeeBean(
            new CoffeeBeanId(Guid.NewGuid()),
            name,
            roaster,
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

    public CoffeeBagMutationResult ReplaceBag(
        CoffeeBagId coffeeBagId,
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
            ? new CoffeeBagNotFound()
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

    public CoffeeBagMutationResult MarkBagOpened(
        CoffeeBagId coffeeBagId,
        DateOnly openedOn,
        DateTimeOffset updatedAt)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        return coffeeBag is null
            ? new CoffeeBagNotFound()
            : coffeeBag.MarkOpened(openedOn, openedOn, updatedAt);
    }

    public CoffeeBagMutationResult SetBagStock(
        CoffeeBagId coffeeBagId,
        bool isInStock,
        DateTimeOffset updatedAt)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        return coffeeBag is null
            ? new CoffeeBagNotFound()
            : coffeeBag.SetStock(isInStock, updatedAt);
    }

    public CoffeeBagMutationResult RemoveBag(CoffeeBagId coffeeBagId)
    {
        var coffeeBag = _coffeeBags.SingleOrDefault(bag => bag.Id == coffeeBagId);
        if (coffeeBag is null)
        {
            return new CoffeeBagNotFound();
        }

        _coffeeBags.Remove(coffeeBag);
        return coffeeBag;
    }

    private static ValidationFailure ValidationFailure(ValidationErrorCode code) =>
        new([new ValidationError(code)]);
}
