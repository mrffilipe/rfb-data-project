using Microsoft.AspNetCore.Mvc;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Common;
using RFBDataProject.Application.Services.Lookups;

namespace RFBDataProject.API.Controllers;

public sealed class LookupsController : V1ApiControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService) => _lookupService = lookupService;

    [HttpGet("states")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public IReadOnlyList<LookupItemDto> ListStates() => _lookupService.ListStates();

    [HttpGet("registration-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public IReadOnlyList<LookupItemDto> ListRegistrationStatuses() => _lookupService.ListRegistrationStatuses();

    [HttpGet("company-sizes")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public IReadOnlyList<LookupItemDto> ListCompanySizes() => _lookupService.ListCompanySizes();

    [HttpGet("cnaes/all")]
    [HttpGet("cnaes/list")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LookupItemDto>> ListCnaes(CancellationToken ct) =>
        await _lookupService.ListCnaesAsync(ct);

    [HttpGet("legal-natures/all")]
    [HttpGet("legal-natures/list")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LookupItemDto>> ListLegalNatures(CancellationToken ct) =>
        await _lookupService.ListLegalNaturesAsync(ct);

    [HttpGet("cnaes")]
    [ProducesResponseType(typeof(PagedResult<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<LookupItemDto>> SearchCnaes([FromQuery] LookupSearchRequest request, CancellationToken ct)
    {
        return await _lookupService.SearchCnaesAsync(request, ct);
    }

    [HttpGet("municipalities")]
    [ProducesResponseType(typeof(PagedResult<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<LookupItemDto>> SearchMunicipalities([FromQuery] LookupSearchRequest request, CancellationToken ct)
    {
        return await _lookupService.SearchMunicipalitiesAsync(request, ct);
    }

    [HttpGet("legal-natures")]
    [ProducesResponseType(typeof(PagedResult<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<LookupItemDto>> SearchLegalNatures([FromQuery] LookupSearchRequest request, CancellationToken ct)
    {
        return await _lookupService.SearchLegalNaturesAsync(request, ct);
    }

    [HttpGet("qualifications")]
    [ProducesResponseType(typeof(PagedResult<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<LookupItemDto>> SearchQualifications([FromQuery] LookupSearchRequest request, CancellationToken ct)
    {
        return await _lookupService.SearchQualificationsAsync(request, ct);
    }
}
