using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBeans;

public sealed class CoffeeBeanTests
{
    [Fact]
    public void CreateBuildsATrimmedProductIdentity()
    {
        var createdAt = new DateTimeOffset(2026, 9, 4, 9, 30, 0, TimeSpan.Zero);

        var result = CoffeeBean.Create(
            ParseName("  Ethiopia Bombe  "),
            ParseRoaster("  Example Roasters  "),
            createdAt);
        var coffeeBean = GetCreatedCoffeeBean(result);

        Assert.False(coffeeBean.Id.IsEmpty());
        Assert.Equal("Ethiopia Bombe", coffeeBean.Name.Value);
        Assert.Equal("Example Roasters", coffeeBean.Roaster.Value);
        Assert.Equal(createdAt, coffeeBean.CreatedAt);
        Assert.Equal(createdAt, coffeeBean.UpdatedAt);
    }

    [Fact]
    public void CreateBuildsTheCompleteProductProfile()
    {
        var result = CoffeeBean.Create(
            ParseName("Ethiopia Bombe"),
            ParseRoaster("Example Roasters"),
            "  Ethiopia, Sidama  ",
            RoastLevel.MediumLight,
            "  Floral and juicy.  ",
            "https://example.com/coffee",
            DateTimeOffset.UtcNow);
        var coffeeBean = GetCreatedCoffeeBean(result);

        Assert.Equal("Ethiopia, Sidama", coffeeBean.Origin);
        Assert.Equal(RoastLevel.MediumLight, coffeeBean.RoastLevel);
        Assert.Equal("Floral and juicy.", coffeeBean.Description);
        Assert.Equal("https://example.com/coffee", coffeeBean.ProductUrl);
    }

    [Theory]
    [InlineData("ftp://example.com/coffee", "coffeeBean.productUrl.invalid")]
    [InlineData("example.com/coffee", "coffeeBean.productUrl.invalid")]
    [InlineData("https://example.com/" + "x", null)]
    public void CreateValidatesTheProductUrl(string productUrl, string? expectedCode)
    {
        if (expectedCode is null)
        {
            productUrl = "https://example.com/" + new string('x', 2_048);
            expectedCode = "coffeeBean.productUrl.tooLong";
        }

        var result = CoffeeBean.Create(
            ParseName("Ethiopia Bombe"),
            ParseRoaster("Example Roasters"),
            null,
            RoastLevel.Unknown,
            null,
            productUrl,
            DateTimeOffset.UtcNow);

        Assert.Equal(expectedCode, GetValidationError(result).Code);
    }

    [Fact]
    public void CreateStoresEmptyOptionalTextAsAbsent()
    {
        var result = CoffeeBean.Create(
            ParseName("Ethiopia Bombe"),
            ParseRoaster("Example Roasters"),
            " ",
            RoastLevel.Unknown,
            " ",
            " ",
            DateTimeOffset.UtcNow);
        var coffeeBean = GetCreatedCoffeeBean(result);

        Assert.Null(coffeeBean.Origin);
        Assert.Null(coffeeBean.Description);
        Assert.Null(coffeeBean.ProductUrl);
    }

    [Theory]
    [InlineData("", "Example Roasters", "coffeeBean.name.required")]
    [InlineData("   ", "Example Roasters", "coffeeBean.name.required")]
    [InlineData("Ethiopia Bombe", "", "coffeeBean.roaster.required")]
    [InlineData("Ethiopia Bombe", "   ", "coffeeBean.roaster.required")]
    public void CreateRejectsMissingProductIdentity(string name, string roaster, string expectedCode)
    {
        var value = string.IsNullOrWhiteSpace(name)
            ? CoffeeBeanName.Parse(name).Value
            : RoasterName.Parse(roaster).Value;
        var error = Assert.IsType<Error>(value);

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(expectedCode, error.Code);
    }

    [Theory]
    [InlineData(true, "coffeeBean.name.tooLong")]
    [InlineData(false, "coffeeBean.roaster.tooLong")]
    public void CreateRejectsProductIdentityLongerThan120Characters(bool nameIsTooLong, string expectedCode)
    {
        var longValue = new string('x', 121);
        var value = nameIsTooLong
            ? CoffeeBeanName.Parse(longValue).Value
            : RoasterName.Parse(longValue).Value;
        var error = Assert.IsType<Error>(value);

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(expectedCode, error.Code);
    }

    private static CoffeeBean GetCreatedCoffeeBean(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean coffeeBean => coffeeBean,
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected a Coffee Bean, but got {failure.Errors.Count} validation error(s)."),
            Error error => throw new InvalidOperationException($"Expected a Coffee Bean, but got {error.Code}.")
        };
    }

    private static Error GetValidationError(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected an Error, but got {failure.Errors.Count} validation error(s)."),
            Error error => error
        };
    }

    private static CoffeeBeanName ParseName(string value) =>
        Assert.IsType<CoffeeBeanName>(CoffeeBeanName.Parse(value).Value);

    private static RoasterName ParseRoaster(string value) =>
        Assert.IsType<RoasterName>(RoasterName.Parse(value).Value);
}
