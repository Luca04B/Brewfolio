using Brewfolio.Domain.BrewingMethods;
using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.Recipes;

public sealed class Recipe
{
    private Recipe()
    {
    }

    private Recipe(
        RecipeId id,
        RecipeName name,
        BrewingMethodId brewingMethodId,
        decimal coffeeAmountInGrams,
        decimal waterAmountInGrams,
        decimal waterTemperatureInCelsius,
        GrindSize grindSize,
        TimeSpan targetBrewTime)
    {
        Id = id;
        Name = name;
        BrewingMethodId = brewingMethodId;
        CoffeeAmountInGrams = coffeeAmountInGrams;
        WaterAmountInGrams = waterAmountInGrams;
        WaterTemperatureInCelsius = waterTemperatureInCelsius;
        GrindSize = grindSize;
        TargetBrewTime = targetBrewTime;
    }

    public RecipeId Id { get; private set; } = null!;

    public RecipeName Name { get; private set; } = null!;

    public BrewingMethodId BrewingMethodId { get; private set; } = null!;

    public decimal CoffeeAmountInGrams { get; private set; }

    public decimal WaterAmountInGrams { get; private set; }

    public decimal WaterTemperatureInCelsius { get; private set; }

    public GrindSize GrindSize { get; private set; }

    public TimeSpan TargetBrewTime { get; private set; }

    public static Result<Recipe> Create(
        RecipeName name,
        BrewingMethodId brewingMethodId,
        decimal coffeeAmountInGrams,
        decimal waterAmountInGrams,
        decimal waterTemperatureInCelsius,
        GrindSize grindSize,
        TimeSpan targetBrewTime)
    {
        List<ValidationError> errors = [];

        if (name is null || string.IsNullOrWhiteSpace(name.Value))
        {
            errors.Add(new ValidationError(
                "recipe.name.required",
                nameof(Name),
                "Name is required."));
        }

        if (brewingMethodId is null || brewingMethodId.IsEmpty())
        {
            errors.Add(new ValidationError(
                "recipe.brewingMethodId.required",
                nameof(BrewingMethodId),
                "Brewing Method id is required."));
        }

        if (coffeeAmountInGrams <= 0)
        {
            errors.Add(new ValidationError(
                "recipe.coffeeAmountInGrams.notPositive",
                nameof(CoffeeAmountInGrams),
                "Coffee amount must be greater than zero."));
        }

        if (waterAmountInGrams <= 0)
        {
            errors.Add(new ValidationError(
                "recipe.waterAmountInGrams.notPositive",
                nameof(WaterAmountInGrams),
                "Water amount must be greater than zero."));
        }

        if (waterTemperatureInCelsius is <= 0 or > 100)
        {
            errors.Add(new ValidationError(
                "recipe.waterTemperatureInCelsius.outOfRange",
                nameof(WaterTemperatureInCelsius),
                "Water temperature must be greater than 0 and at most 100 degrees Celsius."));
        }

        if (!Enum.IsDefined(grindSize))
        {
            errors.Add(new ValidationError(
                "recipe.grindSize.invalid",
                nameof(GrindSize),
                "Grind size is invalid."));
        }

        if (targetBrewTime <= TimeSpan.Zero)
        {
            errors.Add(new ValidationError(
                "recipe.targetBrewTime.notPositive",
                nameof(TargetBrewTime),
                "Target brew time must be greater than zero."));
        }

        if (errors.Count > 0)
        {
            return new ValidationFailure(errors);
        }

        return new Recipe(
            new RecipeId(Guid.NewGuid()),
            new RecipeName(name!.Value.Trim()),
            brewingMethodId!,
            coffeeAmountInGrams,
            waterAmountInGrams,
            waterTemperatureInCelsius,
            grindSize,
            targetBrewTime);
    }
}
