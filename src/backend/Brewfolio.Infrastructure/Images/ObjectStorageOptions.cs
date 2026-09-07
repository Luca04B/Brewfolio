namespace Brewfolio.Infrastructure.Images;

public sealed record ObjectStorageOptions(
    string ServiceUrl,
    string AccessKey,
    string SecretKey,
    string BucketName);
