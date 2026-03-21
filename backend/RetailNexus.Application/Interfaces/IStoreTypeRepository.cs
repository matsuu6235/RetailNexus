using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IStoreTypeRepository
{
    Task<StoreType?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<StoreType?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<StoreType>> ListAsync(string? code, string? name, bool? isActive, CancellationToken ct);
    Task<IReadOnlyList<StoreType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task<int> GetNextDisplayOrderAsync(CancellationToken ct);
    Task AddAsync(StoreType entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}