using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Infrastructure.Persistence;

namespace Brewfolio.Infrastructure.Images;

public sealed class SqlImageCleanupQueue(BrewfolioDbContext dbContext) : IImageCleanupQueue
{
    public async Task EnqueueAsync(string imageKey, CancellationToken cancellationToken)
    {
        dbContext.ImageCleanupJobs.Add(new ImageCleanupJob(imageKey));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
