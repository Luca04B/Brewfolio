using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.CoffeeBrews;

public sealed class CoffeeBrew
{
    private CoffeeBrew()
    {
    }

    private CoffeeBrew(
        Guid id,
        Guid coffeeBeanId,
        Guid recipeId,
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

    public Guid Id { get; private set; }

    public Guid CoffeeBeanId { get; private set; }

    public Guid RecipeId { get; private set; }

    public DateTimeOffset BrewedAt { get; private set; }

    public int? Rating { get; private set; }

    public string? Notes { get; private set; }

    public static Result<CoffeeBrew> Create(
        Guid coffeeBeanId,
        Guid recipeId,
        DateTimeOffset brewedAt,
        int? rating = null,
        string? notes = null)
    {
        List<ValidationError> errors = [];

        if (coffeeBeanId == Guid.Empty)
        {
            errors.Add(new ValidationError(
                "coffeeBrew.coffeeBeanId.required",
                nameof(CoffeeBeanId),
                "Coffee Bean id is required."));
        }

        if (recipeId == Guid.Empty)
        {
            errors.Add(new ValidationError(
                "coffeeBrew.recipeId.required",
                nameof(RecipeId),
                "Recipe id is required."));
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
            Guid.NewGuid(),
            coffeeBeanId,
            recipeId,
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
        return new ValidationError(
            "coffeeBrew.rating.outOfRange",
            nameof(Rating),
            "Rating must be between 1 and 5.");
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
