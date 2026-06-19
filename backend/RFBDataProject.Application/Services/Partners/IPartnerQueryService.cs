using RFBDataProject.Application.Common;
using RFBDataProject.Application.Services.Companies;

namespace RFBDataProject.Application.Services.Partners;

public interface IPartnerQueryService
{
    Task<PagedResult<CompanySummaryDto>> GetCompaniesByPartnerAsync(GetCompaniesByPartnerRequest request, CancellationToken ct = default);
    Task<PagedResult<PartnerDto>> GetPartnersByCnpjAsync(GetPartnersByCnpjRequest request, CancellationToken ct = default);
}
