using RFBDataProject.Domain.Entities;

namespace RFBDataProject.Domain.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByCnpjBaseAsync(string cnpjBase, CancellationToken ct = default);
    Task<bool> ExistsAsync(CancellationToken ct = default);
}
