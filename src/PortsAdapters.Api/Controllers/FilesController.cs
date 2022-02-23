using Microsoft.AspNetCore.Mvc;
using PortsAdapters.Application.FileStorage;

namespace PortsAdapters.Api.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly IFileStorage _fileStorage;
    private readonly string _bucketName;

    public FilesController(ILogger<FilesController> logger, IFileStorage fileStorage, IConfiguration configuration)
    {
        _logger = logger;
        _fileStorage = fileStorage;
        _bucketName = configuration.GetValue<string>("FileStorage:BucketName");
    }

    [HttpPost]
    public async Task<IActionResult> CreateFile([FromForm] IFormFile file)
    {
        var fileName = file.FileName;
        var contentType = file.ContentType;
        var fileStream = file.OpenReadStream();

        await _fileStorage.UploadFileAsync(_bucketName, fileName, contentType, fileStream);

        return Ok();
    }

    [HttpGet("{fileName}/info")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        var fileInfo = await _fileStorage.GetFileInfoAsync(_bucketName, fileName);

        return Ok(fileInfo);
    }

    [HttpGet("{fileName}/download")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        var fileInfo = await _fileStorage.GetFileInfoAsync(_bucketName, fileName);

        if (fileInfo is null)
            return NotFound();

        var contentType = fileInfo.ContentType;
        var downloadName = fileInfo.Name;

        var stream = new MemoryStream();

        await _fileStorage.DownloadFileAsync(_bucketName, fileName, stream);

        stream.Seek(0, SeekOrigin.Begin);

        return File(stream, contentType, downloadName);
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> RemoveFile(string fileName)
    {
        await _fileStorage.RemoveFileAsync(_bucketName, fileName);

        return NoContent();
    }
}
