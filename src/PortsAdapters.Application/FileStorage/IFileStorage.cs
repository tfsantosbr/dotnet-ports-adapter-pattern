using PortsAdapters.Application.FileStorage.Models;

namespace PortsAdapters.Application.FileStorage;

public interface IFileStorage
{
    Task UploadFileAsync(string bucket, string fileName, string contentType, Stream fileStream);
    Task<FileStorageInfo?> GetFileInfoAsync(string bucket, string fileName);
    Task DownloadFileAsync(string bucket, string fileName, Stream destination);
    Task RemoveFileAsync(string bucketName, string fileName);
}