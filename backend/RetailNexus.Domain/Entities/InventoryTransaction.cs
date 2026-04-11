using RetailNexus.Domain.Enums;

namespace RetailNexus.Domain.Entities;

public class InventoryTransaction
{
    public Guid InventoryTransactionId { get; private set; } = Guid.NewGuid();
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public InventoryTransactionType TransactionType { get; private set; }
    public decimal QuantityChange { get; private set; }
    public decimal QuantityAfter { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public Store? Store { get; private set; }
    public Product? Product { get; private set; }

    private InventoryTransaction()
    {
    }

    public InventoryTransaction(
        Guid storeId,
        Guid productId,
        InventoryTransactionType transactionType,
        decimal quantityChange,
        decimal quantityAfter,
        DateTimeOffset occurredAt,
        string? referenceNumber,
        string? note,
        Guid actorUserId)
    {
        StoreId = storeId;
        ProductId = productId;
        TransactionType = transactionType;
        QuantityChange = quantityChange;
        QuantityAfter = quantityAfter;
        OccurredAt = occurredAt;
        ReferenceNumber = referenceNumber;
        Note = string.IsNullOrWhiteSpace(note) ? null : note;
        CreatedBy = actorUserId;
    }
}
