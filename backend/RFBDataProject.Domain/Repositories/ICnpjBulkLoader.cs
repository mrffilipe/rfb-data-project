namespace RFBDataProject.Domain.Repositories;

public interface ICnpjBulkLoader
{
    Task LoadCsvStreamAsync(string targetTable, Stream csvStream, CancellationToken ct = default);
}
