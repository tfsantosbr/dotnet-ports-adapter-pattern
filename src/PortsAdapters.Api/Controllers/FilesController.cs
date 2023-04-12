using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
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

    [HttpPost("multipart")]
    public async Task<IActionResult> CreateFileMultipart([FromForm] IFormFile file)
    {
        var fileName = file.FileName;
        var contentType = file.ContentType;
        var fileStream = file.OpenReadStream();

        await _fileStorage.UploadFileAsync(_bucketName, fileName, contentType, fileStream);

        return Ok();
    }

    [HttpPost("base64")]
    public async Task<IActionResult> CreateFileJson([FromBody] FileData fileData)
    {
        var base64EncodedString = new Regex(@"^data:image\/[a-z]+;base64,").Replace(fileData.Data, "");

        var bytes = Convert.FromBase64String(base64EncodedString);
        Stream contents = new MemoryStream(bytes);

        var contentType = GetMimeType(bytes);

        if (contentType is null)
            return BadRequest("Can't get file mime-type");

        await _fileStorage.UploadFileAsync(_bucketName, fileData.Name, contentType, contents);

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

    private string? GetMimeType(Byte[] fileContentInBytes)
    {
        var inspector = new ContentInspectorBuilder()
        {
            Definitions = MimeDetective.Definitions.Default.All()
        }.Build();

        var results = inspector.Inspect(fileContentInBytes);

        var result = results.FirstOrDefault();

        return result?.Definition.File.MimeType;
    }

    public class FileData
    {
        public string Name { get; set; } = null!;
        public string Data { get; set; } = null!;
    }
}
