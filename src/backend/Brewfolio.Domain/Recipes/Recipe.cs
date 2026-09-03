namespace Brewfolio.Domain.Recipes;

public sealed class Recipe
{
    private Recipe()
    {
    }

    private Recipe(
        Guid id,
        string name,
        Guid brewingMethodId,
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

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public Guid BrewingMethodId { get; private set; }

    public decimal CoffeeAmountInGrams { get; private set; }

    public decimal WaterAmountInGrams { get; private set; }

    public decimal WaterTemperatureInCelsius { get; private set; }

    public GrindSize GrindSize { get; private set; }

    public TimeSpan TargetBrewTime { get; private set; }

    public static Recipe Create(
        string name,
        Guid brewingMethodId,
        decimal coffeeAmountInGrams,
        decimal waterAmountInGrams,
        decimal waterTemperatureInCelsius,
        GrindSize grindSize,
        TimeSpan targetBrewTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (brewingMethodId == Guid.Empty)
        {
            throw new ArgumentException("Brewing Method id is required.", nameof(brewingMethodId));
        }

        EnsurePositive(coffeeAmountInGrams, nameof(coffeeAmountInGrams));
        EnsurePositive(waterAmountInGrams, nameof(waterAmountInGrams));

        if (waterTemperatureInCelsius is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(waterTemperatureInCelsius),
                "Water temperature must be greater than 0 and at most 100 degrees Celsius.");
        }

        if (targetBrewTime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetBrewTime),
                "Target brew time must be greater than zero.");
        }

        return new Recipe(
            Guid.NewGuid(),
            name.Trim(),
            brewingMethodId,
            coffeeAmountInGrams,
            waterAmountInGrams,
            waterTemperatureInCelsius,
            grindSize,
            targetBrewTime);
    }

    private static void EnsurePositive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }
    }
}
