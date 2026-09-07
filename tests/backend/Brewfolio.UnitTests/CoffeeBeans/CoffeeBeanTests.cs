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
    [InlineData("ftp://example.com/coffee")]
    [InlineData("example.com/coffee")]
    public void CreateRejectsAnInvalidProductUrl(string productUrl)
    {
        var result = CoffeeBean.Create(
            ParseName("Ethiopia Bombe"),
            ParseRoaster("Example Roasters"),
            null,
            RoastLevel.Unknown,
            null,
            productUrl,
            DateTimeOffset.UtcNow);

        Assert.Equal(ValidationErrorCode.CoffeeBeanProductUrlInvalid, GetValidationError(result).Code);
    }

    [Fact]
    public void CreateRejectsAProductUrlThatIsTooLong()
    {
        var result = CoffeeBean.Create(
            ParseName("Ethiopia Bombe"),
            ParseRoaster("Example Roasters"),
            null,
            RoastLevel.Unknown,
            null,
            "https://example.com/" + new string('x', 2_048),
            DateTimeOffset.UtcNow);

        Assert.Equal(ValidationErrorCode.CoffeeBeanProductUrlTooLong, GetValidationError(result).Code);
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
    [InlineData("", "Example Roasters", ValidationErrorCode.CoffeeBeanNameRequired)]
    [InlineData("   ", "Example Roasters", ValidationErrorCode.CoffeeBeanNameRequired)]
    [InlineData("Ethiopia Bombe", "", ValidationErrorCode.CoffeeBeanRoasterRequired)]
    [InlineData("Ethiopia Bombe", "   ", ValidationErrorCode.CoffeeBeanRoasterRequired)]
    public void CreateRejectsMissingProductIdentity(
        string name,
        string roaster,
        ValidationErrorCode expectedCode)
    {
        var value = string.IsNullOrWhiteSpace(name)
            ? CoffeeBeanName.Parse(name).Value
            : RoasterName.Parse(roaster).Value;
        var failure = Assert.IsType<ValidationFailure>(value);
        var error = Assert.Single(failure.Errors);

        Assert.Equal(expectedCode, error.Code);
    }

    [Theory]
    [InlineData(true, ValidationErrorCode.CoffeeBeanNameTooLong)]
    [InlineData(false, ValidationErrorCode.CoffeeBeanRoasterTooLong)]
    public void CreateRejectsProductIdentityLongerThan120Characters(
        bool nameIsTooLong,
        ValidationErrorCode expectedCode)
    {
        var longValue = new string('x', 121);
        var value = nameIsTooLong
            ? CoffeeBeanName.Parse(longValue).Value
            : RoasterName.Parse(longValue).Value;
        var failure = Assert.IsType<ValidationFailure>(value);
        var error = Assert.Single(failure.Errors);

        Assert.Equal(expectedCode, error.Code);
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

    private static ValidationError GetValidationError(Result<CoffeeBean> result)
    {
        return result switch
        {
            CoffeeBean => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => Assert.Single(failure.Errors)
        };
    }

    private static CoffeeBeanName ParseName(string value) =>
        Assert.IsType<CoffeeBeanName>(CoffeeBeanName.Parse(value).Value);

    private static RoasterName ParseRoaster(string value) =>
        Assert.IsType<RoasterName>(RoasterName.Parse(value).Value);
}
