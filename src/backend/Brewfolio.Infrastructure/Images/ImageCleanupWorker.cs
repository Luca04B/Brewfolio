using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brewfolio.Infrastructure.Images;

public sealed class ImageCleanupWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<ImageCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<BrewfolioDbContext>();
            var store = scope.ServiceProvider.GetRequiredService<ICoffeeBeanImageStore>();
            var now = timeProvider.GetUtcNow();
            var jobs = await db.ImageCleanupJobs
                .Where(job => job.NextAttemptAt <= now)
                .OrderBy(job => job.NextAttemptAt)
                .Take(20)
                .ToArrayAsync(stoppingToken);
            foreach (var job in jobs)
            {
                try
                {
                    await store.DeleteAsync(job.ImageKey, stoppingToken);
                    db.ImageCleanupJobs.Remove(job);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Image cleanup retry failed for job {JobId}", job.Id);
                    job.Retry(now);
                }
            }
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
