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
            return new ValidationFailure([
                new ValidationError(ValidationErrorCode.CoffeeBeanRoasterRequired)
            ]);
        }

        var normalized = value.Trim();
        return normalized.Length > MaximumLength
            ? new ValidationFailure([
                new ValidationError(ValidationErrorCode.CoffeeBeanRoasterTooLong)
            ])
            : new RoasterName(normalized);
    }
}
