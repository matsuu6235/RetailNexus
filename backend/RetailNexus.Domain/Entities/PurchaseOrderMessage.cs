namespace RetailNexus.Domain.Entities;

public class PurchaseOrderMessage
{
    public Guid PurchaseOrderMessageId { get; private set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; private set; }
    public Guid SentBy { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public PurchaseOrder? PurchaseOrder { get; private set; }
    public User? Sender { get; private set; }

    private PurchaseOrderMessage()
    {
    }

    public PurchaseOrderMessage(Guid purchaseOrderId, Guid sentBy, string body)
    {
        PurchaseOrderId = purchaseOrderId;
        SentBy = sentBy;
        Body = body;
    }
}
