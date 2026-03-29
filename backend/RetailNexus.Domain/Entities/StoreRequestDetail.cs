namespace RetailNexus.Domain.Entities;

public class StoreRequestDetail
{
    public Guid StoreRequestDetailId { get; private set; } = Guid.NewGuid();
    public Guid StoreRequestId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public StoreRequest? StoreRequest { get; private set; }
    public Product? Product { get; private set; }

    private StoreRequestDetail()
    {
    }

    public StoreRequestDetail(
        Guid storeRequestId,
        Guid productId,
        int quantity,
        Guid actorUserId)
    {
        StoreRequestId = storeRequestId;
        ProductId = productId;
        Quantity = quantity;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(int quantity, Guid actorUserId)
    {
        Quantity = quantity;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
