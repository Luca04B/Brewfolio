using Brewfolio.Domain.BrewingMethods;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.BrewingMethods;

public sealed class BrewingMethodTests
{
    [Fact]
    public void CreateBuildsAnActiveBrewingMethod()
    {
        var result = BrewingMethod.Create("V60");
        var brewingMethod = GetCreatedBrewingMethod(result);

        Assert.NotEqual(Guid.Empty, brewingMethod.Id);
        Assert.Equal("V60", brewingMethod.Name);
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void CreateRejectsAnEmptyName()
    {
        Result<BrewingMethod> result = BrewingMethod.Create(" ");
        var failure = GetValidationFailure(result);
        var error = Assert.Single(failure.Errors);

        Assert.Equal("brewingMethod.name.required", error.Code);
        Assert.Equal(nameof(BrewingMethod.Name), error.Field);
    }

    [Fact]
    public void BrewingMethodCanBeDeactivatedAndActivated()
    {
        var result = BrewingMethod.Create("V60");
        var brewingMethod = GetCreatedBrewingMethod(result);

        brewingMethod.Deactivate();
        Assert.False(brewingMethod.IsActive);

        brewingMethod.Activate();
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void RenameRejectsAnEmptyName()
    {
        var result = BrewingMethod.Create("V60");
        var brewingMethod = GetCreatedBrewingMethod(result);

        var renameResult = brewingMethod.Rename(" ");
        var failure = GetValidationFailure(renameResult);
        var error = Assert.Single(failure.Errors);

        Assert.Equal("brewingMethod.name.required", error.Code);
        Assert.Equal(nameof(BrewingMethod.Name), error.Field);
    }

    [Fact]
    public void RenameChangesTheNameAndReturnsSuccess()
    {
        var createResult = BrewingMethod.Create("V60");
        var brewingMethod = GetCreatedBrewingMethod(createResult);

        var result = brewingMethod.Rename("AeroPress");

        Assert.True(result is Success);
        Assert.Equal("AeroPress", brewingMethod.Name);
    }

    private static BrewingMethod GetCreatedBrewingMethod(Result<BrewingMethod> result)
    {
        return result switch
        {
            BrewingMethod brewingMethod => brewingMethod,
            ValidationFailure failure => throw new InvalidOperationException(
                $"Expected a Brewing Method, but got {failure.Errors.Count} validation error(s).")
        };
    }

    private static ValidationFailure GetValidationFailure(Result<BrewingMethod> result)
    {
        return result switch
        {
            BrewingMethod => throw new InvalidOperationException("Expected a validation error."),
            ValidationFailure failure => failure
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
