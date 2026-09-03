using Brewfolio.Domain.CoffeeBrews;

namespace Brewfolio.UnitTests.CoffeeBrews;

public sealed class CoffeeBrewTests
{
    [Fact]
    public void CreateLinksCoffeeBeanAndRecipe()
    {
        var coffeeBeanId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var brewedAt = DateTimeOffset.UtcNow;

        var coffeeBrew = CoffeeBrew.Create(
            coffeeBeanId,
            recipeId,
            brewedAt,
            4,
            "Sweet and balanced");

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
        Assert.Throws<ArgumentOutOfRangeException>(() => CoffeeBrew.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            rating));
    }
}

