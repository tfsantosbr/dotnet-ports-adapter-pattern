using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using PortsAdapters.Application.FileStorage;
using PortsAdapters.Application.FileStorage.Models;

namespace PortsAdapters.ExternalServices.AzureBlobStorage;

public class AzureBlobStorage : IFileStorage
{
    private readonly string _connectionString;

    public AzureBlobStorage(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureStorage");
    }

    public async Task DownloadFileAsync(string bucket, string fileName, Stream destination)
    {
        var blobClient = new BlobClient(_connectionString, bucket, fileName);

        await blobClient.DownloadToAsync(destination);
    }

    public async Task<FileStorageInfo?> GetFileInfoAsync(string bucket, string fileName)
    {
        var blobClient = new BlobClient(_connectionString, bucket, fileName);

        var properies = await blobClient.GetPropertiesAsync();

        if (properies is null)
            return null;

        var fileInfo = new FileStorageInfo
        {
            Id = string.Empty,
            ContentType = properies.Value.ContentType,
            Name = fileName,
            Size = (ulong)properies.Value.ContentLength
        };

        return fileInfo;
    }

    public async Task RemoveFileAsync(string bucket, string fileName)
    {
        var blobClient = new BlobClient(_connectionString, bucket, fileName);

        await blobClient.DeleteAsync();
    }

    public async Task UploadFileAsync(string bucket, string fileName, string contentType, Stream fileStream)
    {
        var blobClient = new BlobClient(_connectionString, bucket, fileName);

        var blobUploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(fileStream, blobUploadOptions);
    }
}
