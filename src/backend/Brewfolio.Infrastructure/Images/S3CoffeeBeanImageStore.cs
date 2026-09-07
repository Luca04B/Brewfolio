using Amazon.S3;
using Amazon.S3.Model;
using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;
using SkiaSharp;

namespace Brewfolio.Infrastructure.Images;

public sealed class S3CoffeeBeanImageStore(
    IAmazonS3 s3,
    ObjectStorageOptions options) : ICoffeeBeanImageStore
{
    private const int MaximumUploadBytes = 5 * 1024 * 1024;

    public async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        try
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = options.BucketName }, cancellationToken);
        }
        catch (AmazonS3Exception exception) when (
            exception.StatusCode is System.Net.HttpStatusCode.Conflict or System.Net.HttpStatusCode.BadRequest)
        {
            // The private bucket already exists.
        }
    }

    public async Task<Result<string>> StoreAsync(
        CoffeeBeanId coffeeBeanId,
        byte[] content,
        CancellationToken cancellationToken)
    {
        if (content.Length == 0 || content.Length > MaximumUploadBytes)
        {
            return Error.Validation(
                "coffeeBean.image.size",
                "Image must be between 1 byte and 5 MB.");
        }

        try
        {
            await EnsureBucketAsync(cancellationToken);
            using var data = SKData.CreateCopy(content);
            using var codec = SKCodec.Create(data);
            if (codec is null || !IsAcceptedFormat(codec.EncodedFormat))
            {
                return Error.Validation(
                    "coffeeBean.image.type",
                    "Image must be JPEG, PNG, or WebP.");
            }

            using var image = SKBitmap.Decode(codec);
            if (image is null)
            {
                return Error.Validation("coffeeBean.image.invalid", "Image content is invalid.");
            }

            var imageKey = $"coffee-beans/{coffeeBeanId.Value:N}/{Guid.NewGuid():N}";
            await PutVariantAsync(image, imageKey, "large", 1_600, cancellationToken);
            try
            {
                await PutVariantAsync(image, imageKey, "thumbnail", 480, cancellationToken);
            }
            catch
            {
                await DeleteAsync(imageKey, cancellationToken);
                throw;
            }

            return imageKey;
        }
        catch (ArgumentException)
        {
            return Error.Validation("coffeeBean.image.invalid", "Image content is invalid.");
        }
        catch (AmazonS3Exception)
        {
            return Error.Validation(
                "coffeeBean.image.storageUnavailable",
                "Image storage is temporarily unavailable.");
        }
    }

    public async Task<StoredCoffeeBeanImage?> OpenAsync(
        string imageKey,
        bool thumbnail,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await s3.GetObjectAsync(
                options.BucketName,
                ObjectKey(imageKey, thumbnail ? "thumbnail" : "large"),
                cancellationToken);
            var copy = new MemoryStream();
            await response.ResponseStream.CopyToAsync(copy, cancellationToken);
            copy.Position = 0;
            return new StoredCoffeeBeanImage(copy, "image/webp");
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (AmazonS3Exception)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string imageKey, CancellationToken cancellationToken)
    {
        await s3.DeleteObjectsAsync(new DeleteObjectsRequest
        {
            BucketName = options.BucketName,
            Objects =
            [
                new KeyVersion { Key = ObjectKey(imageKey, "large") },
                new KeyVersion { Key = ObjectKey(imageKey, "thumbnail") }
            ]
        }, cancellationToken);
    }

    private async Task PutVariantAsync(
        SKBitmap source,
        string imageKey,
        string variant,
        int maximumSize,
        CancellationToken cancellationToken)
    {
        var scale = Math.Min(1d, Math.Min((double)maximumSize / source.Width, (double)maximumSize / source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawBitmap(source, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear));
        using var rendered = surface.Snapshot();
        using var encoded = rendered.Encode(SKEncodedImageFormat.Webp, 85);
        using var stream = new MemoryStream(encoded.ToArray());
        stream.Position = 0;
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = ObjectKey(imageKey, variant),
            ContentType = "image/webp",
            InputStream = stream,
            AutoCloseStream = true
        }, cancellationToken);
    }

    private static bool IsAcceptedFormat(SKEncodedImageFormat format)
    {
        return format is SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp;
    }

    private static string ObjectKey(string imageKey, string variant) => $"{imageKey}/{variant}.webp";
}
