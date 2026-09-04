using Brewfolio.Domain.Results;

namespace Brewfolio.UnitTests.Results;

public sealed class ValidationFailureTests
{
    [Fact]
    public void RequiresAtLeastOneValidationError()
    {
        Assert.Throws<ArgumentException>(() => new ValidationFailure([]));
    }
}
