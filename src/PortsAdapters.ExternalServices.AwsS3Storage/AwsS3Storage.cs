using Amazon.S3;
using Amazon.S3.Model;
using PortsAdapters.Application.FileStorage;
using PortsAdapters.Application.FileStorage.Models;

namespace PortsAdapters.ExternalServices.AwsS3Storage;

public class AwsS3Storage : IFileStorage
{
    private readonly AmazonS3Client _storage;

    public AwsS3Storage()
    {
        _storage = new AmazonS3Client();
    }

    public async Task DownloadFileAsync(string bucketName, string fileName, Stream destination)
    {
        using var storageObject = await _storage.GetObjectAsync(bucketName, fileName);

        await storageObject.ResponseStream.CopyToAsync(destination);
    }

    public async Task<FileStorageInfo?> GetFileInfoAsync(string bucketName, string fileName)
    {
        var storageObject = await _storage.GetObjectMetadataAsync(bucketName, fileName);

        if (storageObject is null)
            return null;

        var fileInfo = new FileStorageInfo
        {
            Id = storageObject.VersionId,
            ContentType = storageObject.Headers.ContentType,
            Name = fileName,
            Size = (ulong)storageObject.ContentLength
        };

        return fileInfo;
    }

    public async Task RemoveFileAsync(string bucketName, string fileName)
    {
        await _storage.DeleteObjectAsync(bucketName, fileName);
    }

    public async Task UploadFileAsync(string bucketName, string fileName, string contentType, Stream fileStream)
    {
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            ContentType = contentType,
            InputStream = fileStream
        };

        var result = await _storage.PutObjectAsync(putObjectRequest);
    }
}