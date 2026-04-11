using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces;

public interface IInventoryTransactionRepository
{
    Task<IReadOnlyList<InventoryTransaction>> ListAsync(
        Guid? storeId,
        Guid? productId,
        InventoryTransactionType? transactionType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int skip,
        int take,
        CancellationToken ct);
    Task<int> CountAsync(
        Guid? storeId,
        Guid? productId,
        InventoryTransactionType? transactionType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken ct);
    Task AddAsync(InventoryTransaction transaction, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
