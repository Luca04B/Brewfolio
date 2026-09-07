using Brewfolio.Domain.Results;
using StrongOf.SourceGeneration;

namespace Brewfolio.Domain.CoffeeBeans;

[Strong<string>]
public partial class CoffeeBeanName
{
    public const int MaximumLength = 120;

    public static Result<CoffeeBeanName> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("coffeeBean.name.required", "Name is required.");
        }

        var normalized = value.Trim();
        return normalized.Length > MaximumLength
            ? Error.Validation("coffeeBean.name.tooLong", "Name cannot exceed 120 characters.")
            : new CoffeeBeanName(normalized);
    }
}
