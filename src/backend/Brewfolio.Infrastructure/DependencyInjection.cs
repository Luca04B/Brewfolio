using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Infrastructure.Persistence;
using Brewfolio.Infrastructure.Persistence.CoffeeBeans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;
using Brewfolio.Infrastructure.Images;

namespace Brewfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBrewfolioInfrastructure(
        this IServiceCollection services,
        string connectionString,
        ObjectStorageOptions? objectStorage = null)
    {
        services.AddDbContext<BrewfolioDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICoffeeBeanRepository, SqlServerCoffeeBeanRepository>();
        if (objectStorage is not null)
        {
            services.AddSingleton(objectStorage);
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                objectStorage.AccessKey,
                objectStorage.SecretKey,
                new AmazonS3Config
                {
                    ServiceURL = objectStorage.ServiceUrl,
                    ForcePathStyle = true,
                    AuthenticationRegion = "us-east-1"
                }));
            services.AddScoped<ICoffeeBeanImageStore, S3CoffeeBeanImageStore>();
            services.AddScoped<IImageCleanupQueue, SqlImageCleanupQueue>();
            services.AddHostedService<ImageCleanupWorker>();
        }
        return services;
    }

    public static async Task MigrateBrewfolioDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrewfolioDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static async Task EnsureBrewfolioObjectStorageAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var imageStore = scope.ServiceProvider.GetService<ICoffeeBeanImageStore>();
        if (imageStore is S3CoffeeBeanImageStore s3Store)
        {
            await s3Store.EnsureBucketAsync(cancellationToken);
        }
    }
}
