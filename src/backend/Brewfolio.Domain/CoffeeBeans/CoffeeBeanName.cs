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
            return new ValidationFailure([
                new ValidationError(ValidationErrorCode.CoffeeBeanNameRequired)
            ]);
        }

        var normalized = value.Trim();
        return normalized.Length > MaximumLength
            ? new ValidationFailure([
                new ValidationError(ValidationErrorCode.CoffeeBeanNameTooLong)
            ])
            : new CoffeeBeanName(normalized);
    }
}
