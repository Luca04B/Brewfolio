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
        var failure = GetValidationFailure(result);
        var error = Assert.Single(failure.Errors);

        Assert.Equal("coffeeBean.price.negative", error.Code);
        Assert.Equal(nameof(CoffeeBean.Price), error.Field);
        Assert.Equal("Price cannot be negative.", error.Description);
    }

    [Fact]
    public void CreateReturnsAllIndependentValidationErrors()
    {
        var result = CoffeeBean.Create(
            " ",
            " ",
            " ",
            (RoastLevel)99,
            -1m);
        var failure = GetValidationFailure(result);

        Assert.Collection(
            failure.Errors,
            error => Assert.Equal("coffeeBean.name.required", error.Code),
            error => Assert.Equal("coffeeBean.roaster.required", error.Code),
            error => Assert.Equal("coffeeBean.origin.required", error.Code),
            error => Assert.Equal("coffeeBean.roastLevel.invalid", error.Code),
            error => Assert.Equal("coffeeBean.price.negative", error.Code));
    }

    [Fact]
    public void CreateAcceptsAnExplicitlyUnknownRoastLevel()
    {
        var result = CoffeeBean.Create(
            "Ethiopia Bombe",
            "Example Roasters",
            "Ethiopia",
            RoastLevel.Unknown,
            18.90m);
        var coffeeBean = GetCreatedCoffeeBean(result);

        Assert.Equal(RoastLevel.Unknown, coffeeBean.RoastLevel);
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
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected a Coffee Bean, but got {failure.Errors.Count} validation error(s).")
        };
    }

    private static ValidationFailure GetValidationFailure(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => failure
        };
    }
}
