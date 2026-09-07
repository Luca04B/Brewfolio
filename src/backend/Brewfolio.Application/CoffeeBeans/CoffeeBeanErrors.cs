using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public enum CoffeeBeanErrorCode
{
    CoffeeBeanNotFound,
    CoffeeBagNotFound,
    DuplicatePossible,
    DuplicateAcknowledgementRequired,
    ImageStorageNotConfigured,
    ImageSizeInvalid,
    ImageTypeUnsupported,
    ImageContentInvalid,
    ImageStorageUnavailable
}

public enum CoffeeBeanErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record CoffeeBeanError(CoffeeBeanErrorCode Code)
{
    public CoffeeBeanErrorType Type => Code switch
    {
        CoffeeBeanErrorCode.CoffeeBeanNotFound or CoffeeBeanErrorCode.CoffeeBagNotFound =>
            CoffeeBeanErrorType.NotFound,
        CoffeeBeanErrorCode.DuplicatePossible or CoffeeBeanErrorCode.DuplicateAcknowledgementRequired =>
            CoffeeBeanErrorType.Conflict,
        CoffeeBeanErrorCode.ImageStorageNotConfigured
            or CoffeeBeanErrorCode.ImageSizeInvalid
            or CoffeeBeanErrorCode.ImageTypeUnsupported
            or CoffeeBeanErrorCode.ImageContentInvalid
            or CoffeeBeanErrorCode.ImageStorageUnavailable => CoffeeBeanErrorType.Validation,
        _ => throw new ArgumentOutOfRangeException(nameof(Code), Code, null)
    };

    public string Description => Code switch
    {
        CoffeeBeanErrorCode.CoffeeBeanNotFound => "Coffee Bean was not found.",
        CoffeeBeanErrorCode.CoffeeBagNotFound => "Coffee Bag was not found.",
        CoffeeBeanErrorCode.DuplicatePossible =>
            "A Coffee Bean with this name and roaster already exists.",
        CoffeeBeanErrorCode.DuplicateAcknowledgementRequired => "Duplication must be acknowledged.",
        CoffeeBeanErrorCode.ImageStorageNotConfigured => "Image storage is unavailable.",
        CoffeeBeanErrorCode.ImageSizeInvalid => "Image must be between 1 byte and 5 MB.",
        CoffeeBeanErrorCode.ImageTypeUnsupported => "Image must be JPEG, PNG, or WebP.",
        CoffeeBeanErrorCode.ImageContentInvalid => "Image content is invalid.",
        CoffeeBeanErrorCode.ImageStorageUnavailable => "Image storage is temporarily unavailable.",
        _ => throw new ArgumentOutOfRangeException(nameof(Code), Code, null)
    };
}

public union CoffeeBeanResult<T>(T, ValidationFailure, CoffeeBeanError);
