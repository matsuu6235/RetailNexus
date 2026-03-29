using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces;

public interface IStoreRequestRepository
{
    Task<StoreRequest?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<StoreRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<StoreRequest>> ListAsync(
        string? requestNumber,
        Guid? fromStoreId,
        Guid? toStoreId,
        StoreRequestStatus? status,
        DateTimeOffset? requestDateFrom,
        DateTimeOffset? requestDateTo,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct);
    Task<int> CountAsync(
        string? requestNumber,
        Guid? fromStoreId,
        Guid? toStoreId,
        StoreRequestStatus? status,
        DateTimeOffset? requestDateFrom,
        DateTimeOffset? requestDateTo,
        bool? isActive,
        CancellationToken ct);
    Task<string?> GetMaxRequestNumberAsync(CancellationToken ct);
    Task AddAsync(StoreRequest request, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    void RemoveDetails(IEnumerable<StoreRequestDetail> details);
    void AddDetail(StoreRequestDetail detail);
}
