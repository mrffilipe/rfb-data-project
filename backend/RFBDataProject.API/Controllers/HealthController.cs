using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace RFBDataProject.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
