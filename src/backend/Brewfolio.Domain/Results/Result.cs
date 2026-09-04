namespace Brewfolio.Domain.Results;

public sealed record ValidationError(string Code, string Field, string Description);

public sealed record ValidationFailure
{
    public ValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException(
                "A validation failure must contain at least one validation error.",
                nameof(errors));
        }

        Errors = Array.AsReadOnly(errors.ToArray());
    }

    public IReadOnlyList<ValidationError> Errors { get; }
}

public readonly record struct Success;

public union Result(Success, ValidationFailure);

public union Result<T>(T, ValidationFailure);
