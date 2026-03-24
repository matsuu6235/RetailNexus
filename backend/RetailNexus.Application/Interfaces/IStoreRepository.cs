using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Store?> GetByCodeAsync(string code, CancellationToken ct);
    Task<int> CountAsync(string? code, string? name, Guid? areaId, Guid? storeTypeId, bool? isActive, CancellationToken ct);
    Task<IReadOnlyList<Store>> ListAsync(string? code, string? name, Guid? areaId, Guid? storeTypeId, bool? isActive, int skip, int take, CancellationToken ct);
    Task<string?> GetMaxStoreCodeAsync(CancellationToken ct);
    Task AddAsync(Store entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}