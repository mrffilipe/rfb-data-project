using RFBDataProject.Application.Common;

namespace RFBDataProject.Application.Services.Companies;

public interface ICompanyQueryService
{
    Task<CompanyDetailDto?> GetByCnpjAsync(GetCompanyByCnpjRequest request, CancellationToken ct = default);
    Task<PagedResult<CompanySummaryDto>> ListHoldingsAsync(ListHoldingsRequest request, CancellationToken ct = default);
}
