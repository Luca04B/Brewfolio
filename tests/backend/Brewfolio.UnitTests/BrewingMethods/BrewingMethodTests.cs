using Brewfolio.Domain.BrewingMethods;

namespace Brewfolio.UnitTests.BrewingMethods;

public sealed class BrewingMethodTests
{
    [Fact]
    public void CreateBuildsAnActiveBrewingMethod()
    {
        var brewingMethod = BrewingMethod.Create("V60");

        Assert.NotEqual(Guid.Empty, brewingMethod.Id);
        Assert.Equal("V60", brewingMethod.Name);
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void BrewingMethodCanBeDeactivatedAndActivated()
    {
        var brewingMethod = BrewingMethod.Create("V60");

        brewingMethod.Deactivate();
        Assert.False(brewingMethod.IsActive);

        brewingMethod.Activate();
        Assert.True(brewingMethod.IsActive);
    }

    [Fact]
    public void RenameRejectsAnEmptyName()
    {
        var brewingMethod = BrewingMethod.Create("V60");

        Assert.Throws<ArgumentException>(() => brewingMethod.Rename(" "));
    }
}

