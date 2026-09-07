using Brewfolio.Domain.BrewingMethods;
using Brewfolio.Domain.Recipes;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.Recipes;

public sealed class RecipeTests
{
    [Fact]
    public void CreateStoresTheRecipeParameters()
    {
        var targetBrewTime = TimeSpan.FromMinutes(3);
        var brewingMethodId = new BrewingMethodId(Guid.NewGuid());

        var result = Recipe.Create(
            new RecipeName("V60 Standard"),
            brewingMethodId,
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            targetBrewTime);
        var recipe = GetCreatedRecipe(result);

        Assert.Equal(brewingMethodId, recipe.BrewingMethodId);
        Assert.Equal(targetBrewTime, recipe.TargetBrewTime);
    }

    [Fact]
    public void CreateRejectsANonPositiveTargetBrewTime()
    {
        var result = Recipe.Create(
            new RecipeName("V60 Standard"),
            new BrewingMethodId(Guid.NewGuid()),
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            TimeSpan.Zero);
        var error = Assert.Single(GetValidationFailure(result).Errors);

        Assert.Equal(ValidationErrorCode.RecipeTargetBrewTimeNotPositive, error.Code);
    }

    [Fact]
    public void CreateRejectsAnEmptyBrewingMethodId()
    {
        var result = Recipe.Create(
            new RecipeName("V60 Standard"),
            new BrewingMethodId(Guid.Empty),
            15m,
            250m,
            94m,
            GrindSize.MediumFine,
            TimeSpan.FromMinutes(3));
        var error = Assert.Single(GetValidationFailure(result).Errors);

        Assert.Equal(ValidationErrorCode.RecipeBrewingMethodRequired, error.Code);
    }

    [Fact]
    public void CreateReturnsAllIndependentValidationErrors()
    {
        Result<Recipe> result = Recipe.Create(
            new RecipeName(" "),
            new BrewingMethodId(Guid.Empty),
            0m,
            0m,
            101m,
            (GrindSize)99,
            TimeSpan.Zero);
        var failure = GetValidationFailure(result);

        Assert.Collection(
            failure.Errors,
            error => Assert.Equal(ValidationErrorCode.RecipeNameRequired, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeBrewingMethodRequired, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeCoffeeAmountNotPositive, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeWaterAmountNotPositive, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeWaterTemperatureOutOfRange, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeGrindSizeInvalid, error.Code),
            error => Assert.Equal(ValidationErrorCode.RecipeTargetBrewTimeNotPositive, error.Code));
    }

    [Fact]
    public void CreateAcceptsAnExplicitlyUnknownGrindSize()
    {
        var result = Recipe.Create(
            new RecipeName("V60 Standard"),
            new BrewingMethodId(Guid.NewGuid()),
            15m,
            250m,
            94m,
            GrindSize.Unknown,
            TimeSpan.FromMinutes(3));
        var recipe = GetCreatedRecipe(result);

        Assert.Equal(GrindSize.Unknown, recipe.GrindSize);
    }

    private static ValidationFailure GetValidationFailure(Result<Recipe> result)
    {
        return result switch
        {
            Recipe => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => failure
        };
    }

    private static Recipe GetCreatedRecipe(Result<Recipe> result)
    {
        return result switch
        {
            Recipe recipe => recipe,
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected a Recipe, but got {failure.Errors.Count} validation error(s).")
        };
    }
}
