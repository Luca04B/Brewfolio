using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.BrewingMethods;

public sealed class BrewingMethod
{
    private BrewingMethod()
    {
    }

    private BrewingMethod(BrewingMethodId id, BrewingMethodName name, bool isActive)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public BrewingMethodId Id { get; private set; } = null!;

    public BrewingMethodName Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static Result<BrewingMethod> Create(BrewingMethodName name)
    {
        if (name is null || string.IsNullOrWhiteSpace(name.Value))
        {
            return InvalidName();
        }

        return new BrewingMethod(
            new BrewingMethodId(Guid.NewGuid()),
            new BrewingMethodName(name.Value.Trim()),
            true);
    }

    public Result Rename(BrewingMethodName name)
    {
        if (name is null || string.IsNullOrWhiteSpace(name.Value))
        {
            return InvalidName();
        }

        Name = new BrewingMethodName(name.Value.Trim());
        return new Success();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static ValidationFailure InvalidName() => new([
        new ValidationError(
            "brewingMethod.name.required",
            nameof(Name),
            "Name is required.")
    ]);
}
