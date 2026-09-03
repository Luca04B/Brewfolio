namespace Brewfolio.Domain.Results;

public enum ErrorType
{
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static Error Validation(string code, string description)
    {
        return new Error(code, description, ErrorType.Validation);
    }
}

public union Result<T>(T, Error);
