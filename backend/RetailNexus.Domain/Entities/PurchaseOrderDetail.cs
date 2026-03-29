namespace RetailNexus.Domain.Entities;

public class PurchaseOrderDetail
{
    public Guid PurchaseOrderDetailId { get; private set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal SubTotal { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public PurchaseOrder? PurchaseOrder { get; private set; }
    public Product? Product { get; private set; }

    private PurchaseOrderDetail()
    {
    }

    public PurchaseOrderDetail(
        Guid purchaseOrderId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        Guid actorUserId)
    {
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SubTotal = quantity * unitPrice;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(int quantity, decimal unitPrice, Guid actorUserId)
    {
        Quantity = quantity;
        UnitPrice = unitPrice;
        SubTotal = quantity * unitPrice;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
