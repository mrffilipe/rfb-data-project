using RFBDataProject.Application.Common;

namespace RFBDataProject.Application.Services.Companies;

public interface ICompanySearchService
{
    Task<PagedResult<CompanySummaryDto>> SearchAsync(SearchCompaniesRequest request, CancellationToken ct = default);
    Task<PagedResult<CompanySummaryDto>> ListAsync(ListCompaniesRequest request, CancellationToken ct = default);
    Task<CompanyExportResultDto> ExportAsync(ExportCompaniesRequest request, CancellationToken ct = default);
}
