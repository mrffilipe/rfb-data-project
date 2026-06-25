using Microsoft.AspNetCore.Mvc;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Common;
using RFBDataProject.Application.Services.Companies;

namespace RFBDataProject.API.Controllers;

public sealed class CompaniesController : V1ApiControllerBase
{
    private readonly ICompanySearchService _searchService;
    private readonly ICompanyQueryService _queryService;

    public CompaniesController(ICompanySearchService searchService, ICompanyQueryService queryService)
    {
        _searchService = searchService;
        _queryService = queryService;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<CompanySummaryDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<CompanySummaryDto>> Search([FromQuery] SearchCompaniesRequest request, CancellationToken ct)
    {
        return await _searchService.SearchAsync(request, ct);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CompanySummaryDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<CompanySummaryDto>> List([FromQuery] ListCompaniesRequest request, CancellationToken ct)
    {
        return await _searchService.ListAsync(request, ct);
    }

    [HttpGet("search/export")]
    [ProducesResponseType(typeof(CompanyExportResultDto), StatusCodes.Status200OK)]
    public async Task<CompanyExportResultDto> ExportSearch([FromQuery] ExportCompaniesRequest request, CancellationToken ct)
    {
        return await _searchService.ExportAsync(request, ct);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(CompanyExportResultDto), StatusCodes.Status200OK)]
    public async Task<CompanyExportResultDto> ExportList([FromQuery] ExportCompaniesRequest request, CancellationToken ct)
    {
        return await _searchService.ExportAsync(request, ct);
    }

    [HttpGet("holdings")]
    [ProducesResponseType(typeof(PagedResult<CompanySummaryDto>), StatusCodes.Status200OK)]
    public async Task<PagedResult<CompanySummaryDto>> Holdings([FromQuery] ListHoldingsRequest request, CancellationToken ct)
    {
        return await _queryService.ListHoldingsAsync(request, ct);
    }

    [HttpGet("{cnpj:regex(^\\d{{14}}$)}")]
    [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDetailDto>> GetByCnpj(string cnpj, CancellationToken ct)
    {
        var result = await _queryService.GetByCnpjAsync(new GetCompanyByCnpjRequest { Cnpj = cnpj }, ct);
        if (result is null)
            return NotFound();

        return Ok(result);
    }
}
