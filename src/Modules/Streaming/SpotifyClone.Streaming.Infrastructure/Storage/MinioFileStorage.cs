using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SpotifyClone.Streaming.Application.Abstractions.Services;

namespace SpotifyClone.Streaming.Infrastructure.Storage;

public class MinioFileStorage : IFileStorage
{
    private readonly MinioOptions _options;
    private readonly IMinioClient _minioClient;

    public MinioFileStorage(IOptions<MinioOptions> options)
    {
        _options = options.Value;

        _minioClient = new MinioClient()
            .WithEndpoint(_options.Endpoint, _options.Port)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task InitializeBucketsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureBucketIsReadyAsync(_options.AudioBucketName, cancellationToken);
        await EnsureBucketIsReadyAsync(_options.ImageBucketName, cancellationToken);
    }

    public string GetImageRootPath()
        => $"/{_options.ImageBucketName}";

    public string GetAudioRootPath()
        => $"/{_options.AudioBucketName}";

    public string GetLocalConversionRootPath()
    {
        if (string.IsNullOrEmpty(_options.LocalScratchPath))
        {
            throw new InvalidOperationException("Local conversion path is not configured.");
        }

        if (!Directory.Exists(_options.LocalScratchPath))
        {
            Directory.CreateDirectory(_options.LocalScratchPath);
        }

        return _options.LocalScratchPath;
    }

    public async Task SaveTempFileToLocal(Stream stream, string relativePath)
    {
        string fullPath = Path.Combine(GetLocalConversionRootPath(), relativePath);
        string directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(
            Path.Combine(fullPath),
            FileMode.Create);

        await stream.CopyToAsync(fileStream);
    }

    public async Task DeleteTempDirectoryFromLocal(string relativePath)
    {
        string fullPath = Path.Combine(GetLocalConversionRootPath(), relativePath);

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
    }

    public async Task SaveAudioFileAsync(Stream stream, string relativePath)
    {
        if (stream.Position != 0)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        long size;
        Stream uploadStream = stream;
        MemoryStream? buffer = null;

        try
        {
            size = stream.Length;
        }
        catch (NotSupportedException)
        {
            buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            uploadStream = buffer;
            size = buffer.Length;
        }

        try
        {
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                .WithBucket(_options.AudioBucketName)
                .WithObject(relativePath)
                .WithStreamData(uploadStream)
                .WithObjectSize(size)
                .WithContentType("audio/mpeg");

            await _minioClient.PutObjectAsync(putObjectArgs);
        }
        finally
        {
            buffer?.Dispose();
        }
    }

    public async Task SaveImageFileAsync(Stream stream, string relativePath)
    {
        if (stream.Position != 0)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
        long size;
        Stream uploadStream = stream;
        MemoryStream? buffer = null;
        try
        {
            size = stream.Length;
        }
        catch (NotSupportedException)
        {
            buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            uploadStream = buffer;
            size = buffer.Length;
        }
        try
        {
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                .WithBucket(_options.ImageBucketName)
                .WithObject(relativePath)
                .WithStreamData(uploadStream)
                .WithObjectSize(size)
                .WithContentType("image/jpeg");
            await _minioClient.PutObjectAsync(putObjectArgs);
        }
        finally
        {
            buffer?.Dispose();
        }
    }

    public async Task DeleteAudioFileAsync(string folderPath)
    {
        string prefix = folderPath.EndsWith('/')
            ? folderPath
            : $"{folderPath}/";

        ListObjectsArgs listArgs = new ListObjectsArgs()
            .WithBucket(_options.AudioBucketName)
            .WithPrefix(prefix)
            .WithRecursive(true);

        IAsyncEnumerable<Item> objects = _minioClient.ListObjectsEnumAsync(listArgs);

        var objectsToDelete = new List<string>();
        await foreach (Item item in objects)
        {
            objectsToDelete.Add(item.Key);
        }

        if (objectsToDelete.Count == 0)
        {
            return;
        }

        RemoveObjectsArgs removeArgs = new RemoveObjectsArgs()
            .WithBucket(_options.AudioBucketName)
            .WithObjects(objectsToDelete);

        await _minioClient.RemoveObjectsAsync(removeArgs);
    }

    public async Task DeleteImageFileAsync(string folderPath)
    {
        string prefix = folderPath.EndsWith('/')
            ? folderPath
            : $"{folderPath}/";

        ListObjectsArgs listArgs = new ListObjectsArgs()
            .WithBucket(_options.ImageBucketName)
            .WithPrefix(prefix)
            .WithRecursive(true);

        IAsyncEnumerable<Item> objects = _minioClient.ListObjectsEnumAsync(listArgs);

        var objectsToDelete = new List<string>();
        await foreach (Item item in objects)
        {
            objectsToDelete.Add(item.Key);
        }

        if (objectsToDelete.Count == 0)
        {
            return;
        }

        RemoveObjectsArgs removeArgs = new RemoveObjectsArgs()
            .WithBucket(_options.AudioBucketName)
            .WithObjects(objectsToDelete);

        await _minioClient.RemoveObjectsAsync(removeArgs);
    }

    public async Task<bool> AudioExists(string relativePath)
    {
        StatObjectArgs args = new StatObjectArgs()
            .WithBucket(_options.AudioBucketName)
            .WithObject(relativePath);
        try
        {
            await _minioClient.StatObjectAsync(args);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task<bool> ImageExists(string relativePath)
    {
        StatObjectArgs args = new StatObjectArgs()
            .WithBucket(_options.ImageBucketName)
            .WithObject(relativePath);
        try
        {
            await _minioClient.StatObjectAsync(args);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task DownloadAudioToLocalFileAsync(string objectName, string localPath)
    {
        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        GetObjectArgs args = new GetObjectArgs()
            .WithBucket(_options.AudioBucketName)
            .WithObject(objectName)
            .WithCallbackStream((stream) =>
            {
                using var fileStream = new FileStream(localPath, FileMode.Create);
                stream.CopyTo(fileStream);
            });

        await _minioClient.GetObjectAsync(args);
    }

    public async Task DownloadImageToLocalFileAsync(string objectName, string localPath)
    {
        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        GetObjectArgs args = new GetObjectArgs()
            .WithBucket(_options.ImageBucketName)
            .WithObject(objectName)
            .WithCallbackStream((stream) =>
            {
                using var fileStream = new FileStream(localPath, FileMode.Create);
                stream.CopyTo(fileStream);
            });
        await _minioClient.GetObjectAsync(args);
    }

    private async Task EnsureBucketIsReadyAsync(string bucketName, CancellationToken ct)
    {
        bool found = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucketName), ct);

        if (found)
        {
            return;
        }

        try
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucketName), ct);

            string policyJson = GetPublicReadPolicy(bucketName);
            await _minioClient.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policyJson), ct);
        }
        catch (Exception ex) when (ex.Message.Contains("already owned by you"))
        {
            // Swallow this specific error: another thread/instance created it
            // between our "found" check and our "MakeBucket" call.
        }
    }

    private static string GetPublicReadPolicy(string bucketName) => $$"""
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": {"AWS": ["*"]},
                "Action": [
                    "s3:GetBucketLocation",
                    "s3:ListBucket",
                    "s3:GetObject"
                ],
                "Resource": [
                    "arn:aws:s3:::{{bucketName}}",
                    "arn:aws:s3:::{{bucketName}}/*"
                ]
            }
        ]
    }
    """;
}
