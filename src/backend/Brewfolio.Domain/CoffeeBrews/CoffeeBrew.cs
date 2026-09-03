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

    public static CoffeeBrew Create(
        Guid coffeeBeanId,
        Guid recipeId,
        DateTimeOffset brewedAt,
        int? rating = null,
        string? notes = null)
    {
        if (coffeeBeanId == Guid.Empty)
        {
            throw new ArgumentException("Coffee Bean id is required.", nameof(coffeeBeanId));
        }

        if (recipeId == Guid.Empty)
        {
            throw new ArgumentException("Recipe id is required.", nameof(recipeId));
        }

        EnsureValidRating(rating);

        return new CoffeeBrew(
            Guid.NewGuid(),
            coffeeBeanId,
            recipeId,
            brewedAt,
            rating,
            notes);
    }

    public void UpdateRating(int? rating)
    {
        EnsureValidRating(rating);
        Rating = rating;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = NormalizeNotes(notes);
    }

    private static void EnsureValidRating(int? rating)
    {
        if (rating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        }
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}

