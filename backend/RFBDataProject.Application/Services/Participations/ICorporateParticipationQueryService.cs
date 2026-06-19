using RFBDataProject.Application.Common;

namespace RFBDataProject.Application.Services.Participations;

public interface ICorporateParticipationQueryService
{
    Task<PagedResult<CorporateParticipationDto>> ListAsync(ListCorporateParticipationsRequest request, CancellationToken ct = default);
}
