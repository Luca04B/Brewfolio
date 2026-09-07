namespace Brewfolio.Domain.Results;

public enum ValidationErrorCode
{
    BrewingMethodNameRequired,
    RecipeNameRequired,
    RecipeBrewingMethodRequired,
    RecipeCoffeeAmountNotPositive,
    RecipeWaterAmountNotPositive,
    RecipeWaterTemperatureOutOfRange,
    RecipeGrindSizeInvalid,
    RecipeTargetBrewTimeNotPositive,
    CoffeeBrewCoffeeBeanRequired,
    CoffeeBrewRecipeRequired,
    CoffeeBrewRatingOutOfRange,
    CoffeeBeanNameRequired,
    CoffeeBeanNameTooLong,
    CoffeeBeanRoasterRequired,
    CoffeeBeanRoasterTooLong,
    CoffeeBeanOriginTooLong,
    CoffeeBeanDescriptionTooLong,
    CoffeeBeanProductUrlTooLong,
    CoffeeBeanProductUrlInvalid,
    CoffeeBagPurchasedDateInFuture,
    CoffeeBagRoastedDateInFuture,
    CoffeeBagOpenedDateInFuture,
    CoffeeBagOpenedBeforeRoasted,
    CoffeeBagWeightNotPositive,
    CoffeeBagPriceNegative
}

public enum ValidationField
{
    BrewingMethodName,
    RecipeName,
    RecipeBrewingMethodId,
    RecipeCoffeeAmountInGrams,
    RecipeWaterAmountInGrams,
    RecipeWaterTemperatureInCelsius,
    RecipeGrindSize,
    RecipeTargetBrewTime,
    CoffeeBrewCoffeeBeanId,
    CoffeeBrewRecipeId,
    CoffeeBrewRating,
    CoffeeBeanName,
    CoffeeBeanRoaster,
    CoffeeBeanOrigin,
    CoffeeBeanDescription,
    CoffeeBeanProductUrl,
    CoffeeBagPurchasedOn,
    CoffeeBagRoastedOn,
    CoffeeBagOpenedOn,
    CoffeeBagInitialWeightGrams,
    CoffeeBagPricePaid
}

public sealed record ValidationError(ValidationErrorCode Code)
{
    public ValidationField Field => Code switch
    {
        ValidationErrorCode.BrewingMethodNameRequired => ValidationField.BrewingMethodName,
        ValidationErrorCode.RecipeNameRequired => ValidationField.RecipeName,
        ValidationErrorCode.RecipeBrewingMethodRequired => ValidationField.RecipeBrewingMethodId,
        ValidationErrorCode.RecipeCoffeeAmountNotPositive => ValidationField.RecipeCoffeeAmountInGrams,
        ValidationErrorCode.RecipeWaterAmountNotPositive => ValidationField.RecipeWaterAmountInGrams,
        ValidationErrorCode.RecipeWaterTemperatureOutOfRange => ValidationField.RecipeWaterTemperatureInCelsius,
        ValidationErrorCode.RecipeGrindSizeInvalid => ValidationField.RecipeGrindSize,
        ValidationErrorCode.RecipeTargetBrewTimeNotPositive => ValidationField.RecipeTargetBrewTime,
        ValidationErrorCode.CoffeeBrewCoffeeBeanRequired => ValidationField.CoffeeBrewCoffeeBeanId,
        ValidationErrorCode.CoffeeBrewRecipeRequired => ValidationField.CoffeeBrewRecipeId,
        ValidationErrorCode.CoffeeBrewRatingOutOfRange => ValidationField.CoffeeBrewRating,
        ValidationErrorCode.CoffeeBeanNameRequired or ValidationErrorCode.CoffeeBeanNameTooLong =>
            ValidationField.CoffeeBeanName,
        ValidationErrorCode.CoffeeBeanRoasterRequired or ValidationErrorCode.CoffeeBeanRoasterTooLong =>
            ValidationField.CoffeeBeanRoaster,
        ValidationErrorCode.CoffeeBeanOriginTooLong => ValidationField.CoffeeBeanOrigin,
        ValidationErrorCode.CoffeeBeanDescriptionTooLong => ValidationField.CoffeeBeanDescription,
        ValidationErrorCode.CoffeeBeanProductUrlTooLong or ValidationErrorCode.CoffeeBeanProductUrlInvalid =>
            ValidationField.CoffeeBeanProductUrl,
        ValidationErrorCode.CoffeeBagPurchasedDateInFuture => ValidationField.CoffeeBagPurchasedOn,
        ValidationErrorCode.CoffeeBagRoastedDateInFuture => ValidationField.CoffeeBagRoastedOn,
        ValidationErrorCode.CoffeeBagOpenedDateInFuture or ValidationErrorCode.CoffeeBagOpenedBeforeRoasted =>
            ValidationField.CoffeeBagOpenedOn,
        ValidationErrorCode.CoffeeBagWeightNotPositive => ValidationField.CoffeeBagInitialWeightGrams,
        ValidationErrorCode.CoffeeBagPriceNegative => ValidationField.CoffeeBagPricePaid,
        _ => throw new ArgumentOutOfRangeException(nameof(Code), Code, null)
    };

    public string Description => Code switch
    {
        ValidationErrorCode.BrewingMethodNameRequired => "Name is required.",
        ValidationErrorCode.RecipeNameRequired => "Name is required.",
        ValidationErrorCode.RecipeBrewingMethodRequired => "Brewing Method id is required.",
        ValidationErrorCode.RecipeCoffeeAmountNotPositive => "Coffee amount must be greater than zero.",
        ValidationErrorCode.RecipeWaterAmountNotPositive => "Water amount must be greater than zero.",
        ValidationErrorCode.RecipeWaterTemperatureOutOfRange =>
            "Water temperature must be greater than 0 and at most 100 degrees Celsius.",
        ValidationErrorCode.RecipeGrindSizeInvalid => "Grind size is invalid.",
        ValidationErrorCode.RecipeTargetBrewTimeNotPositive => "Target brew time must be greater than zero.",
        ValidationErrorCode.CoffeeBrewCoffeeBeanRequired => "Coffee Bean id is required.",
        ValidationErrorCode.CoffeeBrewRecipeRequired => "Recipe id is required.",
        ValidationErrorCode.CoffeeBrewRatingOutOfRange => "Rating must be between 1 and 5.",
        ValidationErrorCode.CoffeeBeanNameRequired => "Name is required.",
        ValidationErrorCode.CoffeeBeanNameTooLong => "Name cannot exceed 120 characters.",
        ValidationErrorCode.CoffeeBeanRoasterRequired => "Roaster is required.",
        ValidationErrorCode.CoffeeBeanRoasterTooLong => "Roaster cannot exceed 120 characters.",
        ValidationErrorCode.CoffeeBeanOriginTooLong => "Origin cannot exceed 240 characters.",
        ValidationErrorCode.CoffeeBeanDescriptionTooLong => "Description cannot exceed 1,000 characters.",
        ValidationErrorCode.CoffeeBeanProductUrlTooLong => "Product URL cannot exceed 2,048 characters.",
        ValidationErrorCode.CoffeeBeanProductUrlInvalid => "Product URL must be a complete HTTP or HTTPS URL.",
        ValidationErrorCode.CoffeeBagPurchasedDateInFuture
            or ValidationErrorCode.CoffeeBagRoastedDateInFuture
            or ValidationErrorCode.CoffeeBagOpenedDateInFuture => "Coffee Bag dates cannot be in the future.",
        ValidationErrorCode.CoffeeBagOpenedBeforeRoasted => "Opened date cannot precede the roast date.",
        ValidationErrorCode.CoffeeBagWeightNotPositive => "Initial weight must be positive.",
        ValidationErrorCode.CoffeeBagPriceNegative => "Price paid cannot be negative.",
        _ => throw new ArgumentOutOfRangeException(nameof(Code), Code, null)
    };
}

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
