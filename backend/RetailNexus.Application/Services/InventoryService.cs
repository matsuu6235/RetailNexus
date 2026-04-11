using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IInventoryTransactionRepository _transactionRepo;

    public InventoryService(
        IInventoryRepository inventoryRepo,
        IInventoryTransactionRepository transactionRepo)
    {
        _inventoryRepo = inventoryRepo;
        _transactionRepo = transactionRepo;
    }

    public async Task<InventoryTransaction> ApplyTransactionAsync(
        Guid storeId,
        Guid productId,
        InventoryTransactionType transactionType,
        decimal quantityChange,
        DateTimeOffset occurredAt,
        string? referenceNumber,
        string? note,
        Guid actorId,
        CancellationToken ct)
    {
        var inventory = await _inventoryRepo.GetByProductAndStoreAsync(productId, storeId, ct);

        if (inventory is null)
        {
            inventory = new Inventory(productId, storeId, 0, actorId);
            await _inventoryRepo.AddAsync(inventory, ct);
        }

        inventory.ApplyQuantityChange(quantityChange, actorId);

        var transaction = new InventoryTransaction(
            storeId,
            productId,
            transactionType,
            quantityChange,
            inventory.Quantity,
            occurredAt,
            referenceNumber,
            note,
            actorId);

        await _transactionRepo.AddAsync(transaction, ct);
        await _inventoryRepo.SaveChangesAsync(ct);

        return transaction;
    }
}
