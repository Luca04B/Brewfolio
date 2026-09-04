using Brewfolio.Domain.CoffeeBrews;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.CoffeeBrews;

public sealed class CoffeeBrewTests
{
    [Fact]
    public void CreateLinksCoffeeBeanAndRecipe()
    {
        var coffeeBeanId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var brewedAt = DateTimeOffset.UtcNow;

        var result = CoffeeBrew.Create(
            coffeeBeanId,
            recipeId,
            brewedAt,
            4,
            "Sweet and balanced");
        var coffeeBrew = GetCreatedCoffeeBrew(result);

        Assert.Equal(coffeeBeanId, coffeeBrew.CoffeeBeanId);
        Assert.Equal(recipeId, coffeeBrew.RecipeId);
        Assert.Equal(brewedAt, coffeeBrew.BrewedAt);
        Assert.Equal(4, coffeeBrew.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CreateRejectsRatingsOutsideTheOneToFiveRange(int rating)
    {
        var result = CoffeeBrew.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            rating);
        var error = Assert.Single(GetValidationFailure(result).Errors);

        Assert.Equal("coffeeBrew.rating.outOfRange", error.Code);
    }

    [Fact]
    public void CreateReturnsAllIndependentValidationErrors()
    {
        Result<CoffeeBrew> result = CoffeeBrew.Create(
            Guid.Empty,
            Guid.Empty,
            DateTimeOffset.UtcNow,
            6);
        var failure = GetValidationFailure(result);

        Assert.Collection(
            failure.Errors,
            error => Assert.Equal("coffeeBrew.coffeeBeanId.required", error.Code),
            error => Assert.Equal("coffeeBrew.recipeId.required", error.Code),
            error => Assert.Equal("coffeeBrew.rating.outOfRange", error.Code));
    }

    [Fact]
    public void UpdateRatingRejectsAnInvalidRatingWithoutChangingTheBrew()
    {
        var createResult = CoffeeBrew.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            4);
        var coffeeBrew = GetCreatedCoffeeBrew(createResult);

        var updateResult = coffeeBrew.UpdateRating(6);
        var failure = GetValidationFailure(updateResult);
        var error = Assert.Single(failure.Errors);

        Assert.Equal("coffeeBrew.rating.outOfRange", error.Code);
        Assert.Equal(4, coffeeBrew.Rating);
    }

    [Fact]
    public void UpdateRatingChangesTheRatingAndReturnsSuccess()
    {
        var createResult = CoffeeBrew.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            4);
        var coffeeBrew = GetCreatedCoffeeBrew(createResult);

        var result = coffeeBrew.UpdateRating(5);

        Assert.True(result is Success);
        Assert.Equal(5, coffeeBrew.Rating);
    }

    private static ValidationFailure GetValidationFailure(Result<CoffeeBrew> result)
    {
        return result switch
        {
            CoffeeBrew => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => failure
        };
    }

    private static CoffeeBrew GetCreatedCoffeeBrew(Result<CoffeeBrew> result)
    {
        return result switch
        {
            CoffeeBrew coffeeBrew => coffeeBrew,
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected a Coffee Brew, but got {failure.Errors.Count} validation error(s).")
        };
    }

    private static ValidationFailure GetValidationFailure(Result result)
    {
        return result switch
        {
            Success => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => failure
        };
    }
}
