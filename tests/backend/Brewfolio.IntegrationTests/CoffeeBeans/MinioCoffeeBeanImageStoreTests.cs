using Amazon.S3;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Infrastructure.Images;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Brewfolio.IntegrationTests.CoffeeBeans;

public sealed class MinioCoffeeBeanImageStoreTests : IAsyncLifetime
{
    private const string AccessKey = "brewfolio-tests";
    private const string SecretKey = "Brewfolio_Test_Secret_12345";
    private readonly IContainer _container = new ContainerBuilder("minio/minio:latest")
        .WithEnvironment("MINIO_ROOT_USER", AccessKey)
        .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
        .WithCommand("server", "/data")
        .WithPortBinding(9000, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(9000))
        .Build();

    [Fact]
    public async Task ValidImageIsStoredAsPrivateLargeAndThumbnailVariants()
    {
        using var s3 = CreateClient();
        var store = new S3CoffeeBeanImageStore(s3, Options());
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        var result = await store.StoreAsync(
            new CoffeeBeanId(Guid.NewGuid()),
            png,
            CancellationToken.None);

        var key = Assert.IsType<string>(result.Value);
        var large = await store.OpenAsync(key, false, CancellationToken.None);
        var thumbnail = await store.OpenAsync(key, true, CancellationToken.None);
        Assert.NotNull(large);
        Assert.NotNull(thumbnail);
        await using var largeScope = large;
        await using var thumbnailScope = thumbnail;
        Assert.Equal("image/webp", large.ContentType);
        Assert.True(large.Content.Length > 0);
        Assert.True(thumbnail.Content.Length > 0);

        await store.DeleteAsync(key, CancellationToken.None);
        Assert.Null(await store.OpenAsync(key, false, CancellationToken.None));
    }

    [Fact]
    public async Task FakeImageContentIsRejectedBeforeStorage()
    {
        using var s3 = CreateClient();
        var store = new S3CoffeeBeanImageStore(s3, Options());

        var result = await store.StoreAsync(
            new CoffeeBeanId(Guid.NewGuid()),
            "not an image"u8.ToArray(),
            CancellationToken.None);

        Assert.Equal("coffeeBean.image.type", Assert.IsType<Brewfolio.Domain.Results.Error>(result.Value).Code);
    }

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    private AmazonS3Client CreateClient() => new(
        AccessKey,
        SecretKey,
        new AmazonS3Config
        {
            ServiceURL = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9000)}",
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1"
        });

    private ObjectStorageOptions Options() => new(
        $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9000)}",
        AccessKey,
        SecretKey,
        "brewfolio-test-images");
}
