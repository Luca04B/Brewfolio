namespace Brewfolio.Domain.BrewingMethods;

public sealed class BrewingMethod
{
    private BrewingMethod()
    {
    }

    private BrewingMethod(Guid id, string name, bool isActive)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static BrewingMethod Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new BrewingMethod(Guid.NewGuid(), name.Trim(), true);
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

