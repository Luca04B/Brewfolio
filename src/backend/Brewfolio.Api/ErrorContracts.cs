using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.Results;

internal enum CoffeeBeanRequestErrorCode
{
    RequestRequired,
    RequestInvalid,
    ImageTooLarge,
    QueryLimitInvalid
}

internal sealed record CoffeeBeanRequestError(
    CoffeeBeanRequestErrorCode Code,
    string Description);

internal union CreateCoffeeBeanRequestParseResult(
    (CreateCoffeeBeanRequest Request, CoffeeBeanImageInput? Image),
    CoffeeBeanRequestError);

internal static class ErrorContract
{
    public static string Code(CoffeeBeanRequestErrorCode code) => code switch
    {
        CoffeeBeanRequestErrorCode.RequestRequired => "coffeeBean.request.required",
        CoffeeBeanRequestErrorCode.RequestInvalid => "coffeeBean.request.invalid",
        CoffeeBeanRequestErrorCode.ImageTooLarge => "coffeeBean.image.size",
        CoffeeBeanRequestErrorCode.QueryLimitInvalid => "coffeeBean.query.limit",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
    };

    public static string Code(CoffeeBeanErrorCode code) => code switch
    {
        CoffeeBeanErrorCode.CoffeeBeanNotFound => "coffeeBean.notFound",
        CoffeeBeanErrorCode.CoffeeBagNotFound => "coffeeBag.notFound",
        CoffeeBeanErrorCode.DuplicatePossible => "coffeeBean.duplicate.possible",
        CoffeeBeanErrorCode.DuplicateAcknowledgementRequired => "coffeeBean.duplicate.acknowledgement",
        CoffeeBeanErrorCode.ImageStorageNotConfigured => "coffeeBean.image.unavailable",
        CoffeeBeanErrorCode.ImageSizeInvalid => "coffeeBean.image.size",
        CoffeeBeanErrorCode.ImageTypeUnsupported => "coffeeBean.image.type",
        CoffeeBeanErrorCode.ImageContentInvalid => "coffeeBean.image.invalid",
        CoffeeBeanErrorCode.ImageStorageUnavailable => "coffeeBean.image.storageUnavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
    };

    public static string Code(ValidationErrorCode code) => code switch
    {
        ValidationErrorCode.BrewingMethodNameRequired => "brewingMethod.name.required",
        ValidationErrorCode.RecipeNameRequired => "recipe.name.required",
        ValidationErrorCode.RecipeBrewingMethodRequired => "recipe.brewingMethodId.required",
        ValidationErrorCode.RecipeCoffeeAmountNotPositive => "recipe.coffeeAmountInGrams.notPositive",
        ValidationErrorCode.RecipeWaterAmountNotPositive => "recipe.waterAmountInGrams.notPositive",
        ValidationErrorCode.RecipeWaterTemperatureOutOfRange => "recipe.waterTemperatureInCelsius.outOfRange",
        ValidationErrorCode.RecipeGrindSizeInvalid => "recipe.grindSize.invalid",
        ValidationErrorCode.RecipeTargetBrewTimeNotPositive => "recipe.targetBrewTime.notPositive",
        ValidationErrorCode.CoffeeBrewCoffeeBeanRequired => "coffeeBrew.coffeeBeanId.required",
        ValidationErrorCode.CoffeeBrewRecipeRequired => "coffeeBrew.recipeId.required",
        ValidationErrorCode.CoffeeBrewRatingOutOfRange => "coffeeBrew.rating.outOfRange",
        ValidationErrorCode.CoffeeBeanNameRequired => "coffeeBean.name.required",
        ValidationErrorCode.CoffeeBeanNameTooLong => "coffeeBean.name.tooLong",
        ValidationErrorCode.CoffeeBeanRoasterRequired => "coffeeBean.roaster.required",
        ValidationErrorCode.CoffeeBeanRoasterTooLong => "coffeeBean.roaster.tooLong",
        ValidationErrorCode.CoffeeBeanOriginTooLong => "coffeeBean.origin.tooLong",
        ValidationErrorCode.CoffeeBeanDescriptionTooLong => "coffeeBean.description.tooLong",
        ValidationErrorCode.CoffeeBeanProductUrlTooLong => "coffeeBean.productUrl.tooLong",
        ValidationErrorCode.CoffeeBeanProductUrlInvalid => "coffeeBean.productUrl.invalid",
        ValidationErrorCode.CoffeeBagPurchasedDateInFuture
            or ValidationErrorCode.CoffeeBagRoastedDateInFuture
            or ValidationErrorCode.CoffeeBagOpenedDateInFuture => "coffeeBag.date.future",
        ValidationErrorCode.CoffeeBagOpenedBeforeRoasted => "coffeeBag.openedOn.beforeRoastedOn",
        ValidationErrorCode.CoffeeBagWeightNotPositive => "coffeeBag.weight.invalid",
        ValidationErrorCode.CoffeeBagPriceNegative => "coffeeBag.price.negative",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
    };

    public static string Field(ValidationField field) => field switch
    {
        ValidationField.BrewingMethodName => "name",
        ValidationField.RecipeName => "name",
        ValidationField.RecipeBrewingMethodId => "brewingMethodId",
        ValidationField.RecipeCoffeeAmountInGrams => "coffeeAmountInGrams",
        ValidationField.RecipeWaterAmountInGrams => "waterAmountInGrams",
        ValidationField.RecipeWaterTemperatureInCelsius => "waterTemperatureInCelsius",
        ValidationField.RecipeGrindSize => "grindSize",
        ValidationField.RecipeTargetBrewTime => "targetBrewTime",
        ValidationField.CoffeeBrewCoffeeBeanId => "coffeeBeanId",
        ValidationField.CoffeeBrewRecipeId => "recipeId",
        ValidationField.CoffeeBrewRating => "rating",
        ValidationField.CoffeeBeanName => "name",
        ValidationField.CoffeeBeanRoaster => "roaster",
        ValidationField.CoffeeBeanOrigin => "origin",
        ValidationField.CoffeeBeanDescription => "description",
        ValidationField.CoffeeBeanProductUrl => "productUrl",
        ValidationField.CoffeeBagPurchasedOn => "purchasedOn",
        ValidationField.CoffeeBagRoastedOn => "roastedOn",
        ValidationField.CoffeeBagOpenedOn => "openedOn",
        ValidationField.CoffeeBagInitialWeightGrams => "initialWeightGrams",
        ValidationField.CoffeeBagPricePaid => "pricePaid",
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
    };
}
