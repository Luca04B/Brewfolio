using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Recipes;
using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.CoffeeBrews;

public sealed class CoffeeBrew
{
    private CoffeeBrew()
    {
    }

    private CoffeeBrew(
        CoffeeBrewId id,
        CoffeeBeanId coffeeBeanId,
        RecipeId recipeId,
        DateTimeOffset brewedAt,
        int? rating,
        string? notes)
    {
        Id = id;
        CoffeeBeanId = coffeeBeanId;
        RecipeId = recipeId;
        BrewedAt = brewedAt;
        Rating = rating;
        Notes = NormalizeNotes(notes);
    }

    public CoffeeBrewId Id { get; private set; } = null!;

    public CoffeeBeanId CoffeeBeanId { get; private set; } = null!;

    public RecipeId RecipeId { get; private set; } = null!;

    public DateTimeOffset BrewedAt { get; private set; }

    public int? Rating { get; private set; }

    public string? Notes { get; private set; }

    public static Result<CoffeeBrew> Create(
        CoffeeBeanId coffeeBeanId,
        RecipeId recipeId,
        DateTimeOffset brewedAt,
        int? rating = null,
        string? notes = null)
    {
        List<ValidationError> errors = [];

        if (coffeeBeanId is null || coffeeBeanId.IsEmpty())
        {
            errors.Add(new ValidationError(ValidationErrorCode.CoffeeBrewCoffeeBeanRequired));
        }

        if (recipeId is null || recipeId.IsEmpty())
        {
            errors.Add(new ValidationError(ValidationErrorCode.CoffeeBrewRecipeRequired));
        }

        if (!IsValidRating(rating))
        {
            errors.Add(InvalidRating());
        }

        if (errors.Count > 0)
        {
            return new ValidationFailure(errors);
        }

        return new CoffeeBrew(
            new CoffeeBrewId(Guid.NewGuid()),
            coffeeBeanId!,
            recipeId!,
            brewedAt,
            rating,
            notes);
    }

    public Result UpdateRating(int? rating)
    {
        if (!IsValidRating(rating))
        {
            return new ValidationFailure([InvalidRating()]);
        }

        Rating = rating;
        return new Success();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = NormalizeNotes(notes);
    }

    private static bool IsValidRating(int? rating)
    {
        return rating is null or >= 1 and <= 5;
    }

    private static ValidationError InvalidRating()
    {
        return new ValidationError(ValidationErrorCode.CoffeeBrewRatingOutOfRange);
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
