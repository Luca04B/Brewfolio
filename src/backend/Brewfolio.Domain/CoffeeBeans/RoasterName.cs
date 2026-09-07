using Brewfolio.Domain.Results;
using StrongOf.SourceGeneration;

namespace Brewfolio.Domain.CoffeeBeans;

[Strong<string>]
public partial class RoasterName
{
    public const int MaximumLength = 120;

    public static Result<RoasterName> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("coffeeBean.roaster.required", "Roaster is required.");
        }

        var normalized = value.Trim();
        return normalized.Length > MaximumLength
            ? Error.Validation("coffeeBean.roaster.tooLong", "Roaster cannot exceed 120 characters.")
            : new RoasterName(normalized);
    }
}
