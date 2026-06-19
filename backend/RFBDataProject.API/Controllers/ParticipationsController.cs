using Microsoft.AspNetCore.Mvc;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Common;
using RFBDataProject.Application.Services.Participations;

namespace RFBDataProject.API.Controllers;

public sealed class ParticipationsController : V1ApiControllerBase
{
    private readonly ICorporateParticipationQueryService _participationQueryService;

    public ParticipationsController(ICorporateParticipationQueryService participationQueryService) =>
        _participationQueryService = participationQueryService;

    [HttpGet("corporate")]
    [ProducesResponseType(typeof(PagedResult<CorporateParticipationDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<CorporateParticipationDto>> ListCorporate([FromQuery] ListCorporateParticipationsRequest request, CancellationToken ct)
    {
        return await _participationQueryService.ListAsync(request, ct);
    }
}
