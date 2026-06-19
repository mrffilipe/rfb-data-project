namespace RFBDataProject.Domain.Repositories;

public interface IPartnerRepository
{
    Task<bool> ExistsAsync(CancellationToken ct = default);
}
