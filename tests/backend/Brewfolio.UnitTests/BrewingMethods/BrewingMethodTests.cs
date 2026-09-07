using Brewfolio.Domain.BrewingMethods;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.BrewingMethods;

public sealed class BrewingMethodTests
{
    [Fact]
    public void CreateBuildsAnActiveBrewingMethod()
    {
        var result = BrewingMethod.Create(new BrewingMethodName("V60"));
        var brewingMethod = GetCreatedBrewingMethod(result);

        Assert.False(brewingMethod.Id.IsEmpty());
        Assert.Equal("V60", brewingMethod.Name.Value);
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void CreateRejectsAnEmptyName()
    {
        Result<BrewingMethod> result = BrewingMethod.Create(new BrewingMethodName(" "));
        var failure = GetValidationFailure(result);
        var error = Assert.Single(failure.Errors);

        Assert.Equal(ValidationErrorCode.BrewingMethodNameRequired, error.Code);
        Assert.Equal(ValidationField.BrewingMethodName, error.Field);
    }

    [Fact]
    public void BrewingMethodCanBeDeactivatedAndActivated()
    {
        var result = BrewingMethod.Create(new BrewingMethodName("V60"));
        var brewingMethod = GetCreatedBrewingMethod(result);

        brewingMethod.Deactivate();
        Assert.False(brewingMethod.IsActive);

        brewingMethod.Activate();
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void RenameRejectsAnEmptyName()
    {
        var result = BrewingMethod.Create(new BrewingMethodName("V60"));
        var brewingMethod = GetCreatedBrewingMethod(result);

        var renameResult = brewingMethod.Rename(new BrewingMethodName(" "));
        var failure = GetValidationFailure(renameResult);
        var error = Assert.Single(failure.Errors);

        Assert.Equal(ValidationErrorCode.BrewingMethodNameRequired, error.Code);
        Assert.Equal(ValidationField.BrewingMethodName, error.Field);
    }

    [Fact]
    public void RenameChangesTheNameAndReturnsSuccess()
    {
        var createResult = BrewingMethod.Create(new BrewingMethodName("V60"));
        var brewingMethod = GetCreatedBrewingMethod(createResult);

        var result = brewingMethod.Rename(new BrewingMethodName("AeroPress"));

        Assert.True(result is Success);
        Assert.Equal("AeroPress", brewingMethod.Name.Value);
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
