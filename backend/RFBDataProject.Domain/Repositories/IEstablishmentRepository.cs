using RFBDataProject.Domain.Entities;

namespace RFBDataProject.Domain.Repositories;

public interface IEstablishmentRepository
{
    Task<Establishment?> GetByCnpjAsync(string cnpj, CancellationToken ct = default);
}
