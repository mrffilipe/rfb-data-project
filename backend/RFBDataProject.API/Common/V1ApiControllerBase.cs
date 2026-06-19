using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace RFBDataProject.API.Common;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class V1ApiControllerBase : ControllerBase
{
}
