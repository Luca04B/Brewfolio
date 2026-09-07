using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.Results;

public sealed class ValidationFailureTests
{
    [Fact]
    public void RequiresAtLeastOneValidationError()
    {
        Assert.Throws<ArgumentException>(() => new ValidationFailure([]));
    }

    [Fact]
    public void EveryValidationCodeDefinesItsFieldAndDescription()
    {
        foreach (var code in Enum.GetValues<ValidationErrorCode>())
        {
            var error = new ValidationError(code);

            Assert.True(Enum.IsDefined(error.Field));
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
        }
    }

    [Fact]
    public void EveryCoffeeBeanErrorCodeDefinesItsTypeAndDescription()
    {
        foreach (var code in Enum.GetValues<CoffeeBeanErrorCode>())
        {
            var error = new CoffeeBeanError(code);

            Assert.True(Enum.IsDefined(error.Type));
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
        }
    }
}
