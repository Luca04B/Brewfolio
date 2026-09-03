using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBeans;

public sealed class CoffeeBeanTests
{
    [Fact]
    public void CreateBuildsAnInStockCoffeeBean()
    {
        var result = CoffeeBean.Create(
            "Ethiopia Bombe",
            "Example Roasters",
            "Ethiopia",
            RoastLevel.Light,
            18.90m);
        var coffeeBean = GetCreatedCoffeeBean(result);

        Assert.NotEqual(Guid.Empty, coffeeBean.Id);
        Assert.Equal(18.90m, coffeeBean.Price);
        Assert.True(coffeeBean.IsInStock);
    }

    [Fact]
    public void CreateRejectsANegativePrice()
    {
        var result = CoffeeBean.Create(
            "Ethiopia Bombe",
            "Example Roasters",
            "Ethiopia",
            RoastLevel.Light,
            -1m);
        var error = GetValidationError(result);

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("coffeeBean.price.negative", error.Code);
        Assert.Equal("Price cannot be negative.", error.Description);
    }

    [Fact]
    public void StockCanBeChanged()
    {
        var result = CoffeeBean.Create(
            "Ethiopia Bombe",
            "Example Roasters",
            "Ethiopia",
            RoastLevel.Light,
            18.90m);
        var coffeeBean = GetCreatedCoffeeBean(result);

        coffeeBean.MarkAsOutOfStock();
        Assert.False(coffeeBean.IsInStock);

        coffeeBean.MarkAsInStock();
        Assert.True(coffeeBean.IsInStock);
    }

    private static CoffeeBean GetCreatedCoffeeBean(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean coffeeBean => coffeeBean,
            Error error => throw new InvalidOperationException($"Expected a Coffee Bean, but got {error.Code}.")
        };
    }

    private static Error GetValidationError(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean => throw new InvalidOperationException("Expected a validation error."),
            Error error => error
        };
    }
}
