using RFBDataProject.Application.Common;

namespace RFBDataProject.Application.Services.Lookups;

public interface ILookupService
{
    IReadOnlyList<LookupItemDto> ListStates();
    IReadOnlyList<LookupItemDto> ListRegistrationStatuses();
    IReadOnlyList<LookupItemDto> ListCompanySizes();

    Task<PagedResult<LookupItemDto>> SearchCnaesAsync(LookupSearchRequest request, CancellationToken ct = default);
    Task<PagedResult<LookupItemDto>> SearchMunicipalitiesAsync(LookupSearchRequest request, CancellationToken ct = default);
    Task<PagedResult<LookupItemDto>> SearchLegalNaturesAsync(LookupSearchRequest request, CancellationToken ct = default);
    Task<PagedResult<LookupItemDto>> SearchQualificationsAsync(LookupSearchRequest request, CancellationToken ct = default);
}
