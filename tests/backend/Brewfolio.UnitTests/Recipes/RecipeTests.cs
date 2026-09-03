using Brewfolio.Domain.Recipes;

namespace Brewfolio.UnitTests.Recipes;

public sealed class RecipeTests
{
    [Fact]
    public void CreateStoresTheRecipeParameters()
    {
        var targetBrewTime = TimeSpan.FromMinutes(3);
        var brewingMethodId = Guid.NewGuid();

        var recipe = Recipe.Create(
            "V60 Standard",
            brewingMethodId,
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            targetBrewTime);

        Assert.Equal(brewingMethodId, recipe.BrewingMethodId);
        Assert.Equal(targetBrewTime, recipe.TargetBrewTime);
    }

    [Fact]
    public void CreateRejectsANonPositiveTargetBrewTime()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Recipe.Create(
            "V60 Standard",
            Guid.NewGuid(),
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            TimeSpan.Zero));
    }

    [Fact]
    public void CreateRejectsAnEmptyBrewingMethodId()
    {
        Assert.Throws<ArgumentException>(() => Recipe.Create(
            "V60 Standard",
            Guid.Empty,
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            TimeSpan.FromMinutes(3)));
    }
}

