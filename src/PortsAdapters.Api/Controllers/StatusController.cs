using Microsoft.AspNetCore.Mvc;
using PortsAdapters.Application.FileStorage;

namespace PortsAdapters.Api.Controllers;

[ApiController]
[Route("status")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Status()
    {
        return Ok("Ok");
    }
}
