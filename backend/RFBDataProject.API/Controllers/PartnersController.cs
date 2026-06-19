using Microsoft.AspNetCore.Mvc;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Common;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Application.Services.Partners;

namespace RFBDataProject.API.Controllers;

public sealed class PartnersController : V1ApiControllerBase
{
    private readonly IPartnerQueryService _partnerQueryService;

    public PartnersController(IPartnerQueryService partnerQueryService) => _partnerQueryService = partnerQueryService;

    [HttpGet("{document}/companies")]
    [ProducesResponseType(typeof(PagedResult<CompanySummaryDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<CompanySummaryDto>> GetCompaniesByPartner(
        string document,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return await _partnerQueryService.GetCompaniesByPartnerAsync(new GetCompaniesByPartnerRequest
        {
            Document = document,
            Page = page,
            PageSize = pageSize
        }, ct);
    }

    [HttpGet("by-cnpj/{cnpj}")]
    [ProducesResponseType(typeof(PagedResult<PartnerDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<PartnerDto>> GetPartnersByCnpj(
        string cnpj,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return await _partnerQueryService.GetPartnersByCnpjAsync(new GetPartnersByCnpjRequest
        {
            Cnpj = cnpj,
            Page = page,
            PageSize = pageSize
        }, ct);
    }
}
