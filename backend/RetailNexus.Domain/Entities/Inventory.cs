namespace RetailNexus.Domain.Entities;

public class Inventory
{
    public Guid InventoryId { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Guid StoreId { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public Product? Product { get; private set; }
    public Store? Store { get; private set; }

    private Inventory()
    {
    }

    public Inventory(Guid productId, Guid storeId, decimal quantity, Guid actorUserId)
    {
        ProductId = productId;
        StoreId = storeId;
        Quantity = quantity;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void ApplyQuantityChange(decimal quantityChange, Guid actorUserId)
    {
        Quantity += quantityChange;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetQuantity(decimal quantity, Guid actorUserId)
    {
        Quantity = quantity;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
