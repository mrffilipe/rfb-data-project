using Microsoft.AspNetCore.Mvc;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Services.Ingestion;

namespace RFBDataProject.API.Controllers;

public sealed class IngestionController : V1ApiControllerBase
{
    private readonly IIngestionService _ingestionService;

    public IngestionController(IIngestionService ingestionService) => _ingestionService = ingestionService;

    [HttpGet("status")]
    [ProducesResponseType(typeof(IngestionStatusDto), StatusCodes.Status200OK)]
    public async Task<IngestionStatusDto> GetStatus(CancellationToken ct)
    {
        return await _ingestionService.GetStatusAsync(ct);
    }

    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TriggerSync(CancellationToken ct)
    {
        await _ingestionService.TriggerSyncAsync(ct);
        var status = await _ingestionService.GetStatusAsync(ct);
        return Accepted(status);
    }
}
