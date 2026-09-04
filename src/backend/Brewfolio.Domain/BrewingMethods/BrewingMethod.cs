using Brewfolio.Domain.Results;

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

    public static Result<BrewingMethod> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationFailure([
                new ValidationError(
                    "brewingMethod.name.required",
                    nameof(Name),
                    "Name is required.")
            ]);
        }

        return new BrewingMethod(Guid.NewGuid(), name.Trim(), true);
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationFailure([
                new ValidationError(
                    "brewingMethod.name.required",
                    nameof(Name),
                    "Name is required.")
            ]);
        }

        Name = name.Trim();
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
}
