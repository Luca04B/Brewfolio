using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.CoffeeBeans;

public sealed class CoffeeBean
{
    private CoffeeBean()
    {
    }

    private CoffeeBean(
        Guid id,
        string name,
        string roaster,
        string origin,
        RoastLevel roastLevel,
        decimal price,
        bool isInStock)
    {
        Id = id;
        Name = name;
        Roaster = roaster;
        Origin = origin;
        RoastLevel = roastLevel;
        Price = price;
        IsInStock = isInStock;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string Roaster { get; private set; } = null!;

    public string Origin { get; private set; } = null!;

    public RoastLevel RoastLevel { get; private set; }

    public decimal Price { get; private set; }

    public bool IsInStock { get; private set; }

    public static Result<CoffeeBean> Create(
        string name,
        string roaster,
        string origin,
        RoastLevel roastLevel,
        decimal price,
        bool isInStock = true)
    {
        List<ValidationError> errors = [];

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(new ValidationError(
                "coffeeBean.name.required",
                nameof(Name),
                "Name is required."));
        }

        if (string.IsNullOrWhiteSpace(roaster))
        {
            errors.Add(new ValidationError(
                "coffeeBean.roaster.required",
                nameof(Roaster),
                "Roaster is required."));
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            errors.Add(new ValidationError(
                "coffeeBean.origin.required",
                nameof(Origin),
                "Origin is required."));
        }

        if (!Enum.IsDefined(roastLevel))
        {
            errors.Add(new ValidationError(
                "coffeeBean.roastLevel.invalid",
                nameof(RoastLevel),
                "Roast level is invalid."));
        }

        if (price < 0)
        {
            errors.Add(new ValidationError(
                "coffeeBean.price.negative",
                nameof(Price),
                "Price cannot be negative."));
        }

        if (errors.Count > 0)
        {
            return new ValidationFailure(errors);
        }

        return new CoffeeBean(
            Guid.NewGuid(),
            name.Trim(),
            roaster.Trim(),
            origin.Trim(),
            roastLevel,
            price,
            isInStock);
    }

    public void MarkAsInStock()
    {
        IsInStock = true;
    }

    public void MarkAsOutOfStock()
    {
        IsInStock = false;
    }
}
