using Brewfolio.Application;
using Brewfolio.Domain;
using Brewfolio.Infrastructure;

namespace Brewfolio.ArchitectureTests;

public sealed class ProjectDependencyTests
{
    [Fact]
    public void DomainDoesNotReferenceOuterProjects()
    {
        var references = GetProjectReferences(typeof(DomainAssembly));

        Assert.DoesNotContain("Brewfolio.Application", references);
        Assert.DoesNotContain("Brewfolio.Infrastructure", references);
        Assert.DoesNotContain("Brewfolio.Api", references);
    }

    [Fact]
    public void ApplicationDoesNotReferenceInfrastructureOrApi()
    {
        var references = GetProjectReferences(typeof(ApplicationAssembly));

        Assert.DoesNotContain("Brewfolio.Infrastructure", references);
        Assert.DoesNotContain("Brewfolio.Api", references);
    }

    [Fact]
    public void InfrastructureDoesNotReferenceApi()
    {
        var references = GetProjectReferences(typeof(InfrastructureAssembly));

        Assert.DoesNotContain("Brewfolio.Api", references);
    }

    private static string[] GetProjectReferences(Type assemblyMarker)
    {
        return assemblyMarker.Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith("Brewfolio.", StringComparison.Ordinal))
            .Cast<string>()
            .ToArray();
    }
}

