using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBags;

public sealed class CoffeeBagTests
{
    private static readonly DateOnly Today = new(2026, 9, 4);
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CoffeeBeanOwnsAValidCoffeeBag()
    {
        var coffeeBean = CreateCoffeeBean();

        var result = coffeeBean.AddBag(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 8, 28),
            null,
            250,
            0m,
            true,
            Today,
            Now);
        var coffeeBag = Assert.IsType<CoffeeBag>(result.Value);

        Assert.Equal(coffeeBean.Id, coffeeBag.CoffeeBeanId);
        Assert.Equal(250, coffeeBag.InitialWeightGrams);
        Assert.Equal(0m, coffeeBag.PricePaid);
        Assert.True(coffeeBag.IsInStock);
        Assert.Contains(coffeeBag, coffeeBean.CoffeeBags);
    }

    [Theory]
    [InlineData(-1, 10, "coffeeBag.weight.invalid")]
    [InlineData(0, 10, "coffeeBag.weight.invalid")]
    [InlineData(250, -1, "coffeeBag.price.negative")]
    public void CoffeeBagRejectsInvalidQuantityOrPrice(
        int weight,
        decimal price,
        string expectedCode)
    {
        var result = CreateCoffeeBean().AddBag(
            Today,
            null,
            null,
            weight,
            price,
            true,
            Today,
            Now);

        Assert.Equal(expectedCode, Assert.IsType<Error>(result.Value).Code);
    }

    [Fact]
    public void CoffeeBagRejectsFutureDates()
    {
        var result = CreateCoffeeBean().AddBag(
            Today.AddDays(1),
            null,
            null,
            250,
            10m,
            true,
            Today,
            Now);

        Assert.Equal("coffeeBag.date.future", Assert.IsType<Error>(result.Value).Code);
    }

    [Fact]
    public void CoffeeBagRejectsOpeningBeforeKnownRoastDate()
    {
        var result = CreateCoffeeBean().AddBag(
            Today,
            Today.AddDays(-2),
            Today.AddDays(-3),
            250,
            10m,
            true,
            Today,
            Now);

        Assert.Equal("coffeeBag.openedOn.beforeRoastedOn", Assert.IsType<Error>(result.Value).Code);
    }

    [Fact]
    public void CoffeeBagCanBeCorrectedOpenedAndTakenOutOfStock()
    {
        var coffeeBean = CreateCoffeeBean();
        var created = coffeeBean.AddBag(Today, null, null, 250, 10m, true, Today, Now);
        var coffeeBag = Assert.IsType<CoffeeBag>(created.Value);
        var later = Now.AddHours(1);

        Assert.IsType<CoffeeBag>(coffeeBean.ReplaceBag(
            coffeeBag.Id,
            Today.AddDays(-1),
            Today.AddDays(-3),
            null,
            500,
            20m,
            true,
            Today,
            later).Value);
        Assert.IsType<CoffeeBag>(coffeeBean.MarkBagOpened(coffeeBag.Id, Today, later).Value);
        Assert.IsType<CoffeeBag>(coffeeBean.SetBagStock(coffeeBag.Id, false, later).Value);

        Assert.Equal(500, coffeeBag.InitialWeightGrams);
        Assert.Equal(Today, coffeeBag.OpenedOn);
        Assert.False(coffeeBag.IsInStock);
        Assert.Equal(later, coffeeBag.UpdatedAt);
    }

    [Fact]
    public void CoffeeBagCanBeRemovedOnlyFromItsOwningCoffeeBean()
    {
        var coffeeBean = CreateCoffeeBean();

        var missing = coffeeBean.RemoveBag(new CoffeeBagId(Guid.NewGuid()));

        Assert.Equal("coffeeBag.notFound", Assert.IsType<Error>(missing.Value).Code);
        Assert.Empty(coffeeBean.CoffeeBags);
    }

    private static CoffeeBean CreateCoffeeBean()
    {
        var result = CoffeeBean.Create(
            new CoffeeBeanName("Bombe"),
            new RoasterName("Example"),
            Now);
        return Assert.IsType<CoffeeBean>(result.Value);
    }
}
