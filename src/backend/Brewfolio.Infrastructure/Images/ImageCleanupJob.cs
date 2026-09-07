namespace Brewfolio.Infrastructure.Images;

public sealed class ImageCleanupJob
{
    private ImageCleanupJob() { }
    public ImageCleanupJob(string imageKey)
    {
        Id = Guid.NewGuid();
        ImageKey = imageKey;
        NextAttemptAt = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public string ImageKey { get; private set; } = null!;
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public void Retry(DateTimeOffset now)
    {
        AttemptCount++;
        NextAttemptAt = now.AddMinutes(Math.Min(60, Math.Pow(2, AttemptCount)));
    }
}
