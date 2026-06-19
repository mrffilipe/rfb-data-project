using RFBDataProject.Application.Interfaces;

namespace RFBDataProject.Application.Common;

public sealed record PagedResult<T> : IPaged
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public required IReadOnlyList<T> Items { get; init; }
}
