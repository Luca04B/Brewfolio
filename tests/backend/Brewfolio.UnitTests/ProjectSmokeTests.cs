using Brewfolio.Domain;

namespace Brewfolio.UnitTests;

public sealed class ProjectSmokeTests
{
    [Fact]
    public void DomainAssemblyCanBeLoaded()
    {
        var assemblyName = typeof(DomainAssembly).Assembly.GetName().Name;

        Assert.Equal("Brewfolio.Domain", assemblyName);
    }
}

