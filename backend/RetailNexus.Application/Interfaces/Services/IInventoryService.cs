using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryTransaction> ApplyTransactionAsync(
        Guid storeId,
        Guid productId,
        InventoryTransactionType transactionType,
        decimal quantityChange,
        DateTimeOffset occurredAt,
        string? referenceNumber,
        string? note,
        Guid actorId,
        CancellationToken ct);
}
