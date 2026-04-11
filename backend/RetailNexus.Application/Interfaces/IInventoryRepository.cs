using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductAndStoreAsync(Guid productId, Guid storeId, CancellationToken ct);
    Task<IReadOnlyList<Inventory>> ListAsync(
        Guid? areaId,
        Guid? storeId,
        Guid? productCategoryId,
        string? productCode,
        Guid? supplierId,
        string? stockStatus,
        int skip,
        int take,
        CancellationToken ct);
    Task<int> CountAsync(
        Guid? areaId,
        Guid? storeId,
        Guid? productCategoryId,
        string? productCode,
        Guid? supplierId,
        string? stockStatus,
        CancellationToken ct);
    Task AddAsync(Inventory inventory, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
