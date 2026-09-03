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
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("coffeeBean.name.required", "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(roaster))
        {
            return Error.Validation("coffeeBean.roaster.required", "Roaster is required.");
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            return Error.Validation("coffeeBean.origin.required", "Origin is required.");
        }

        if (price < 0)
        {
            return Error.Validation("coffeeBean.price.negative", "Price cannot be negative.");
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
