namespace RetailNexus.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 0,
    AwaitingApproval = 1,
    Approved = 2,
    SupplierConfirmed = 3,
    Preparing = 4,
    Shipped = 5,
    Received = 6,
    CancelRequested = 91,
    Cancelled = 92,
    SupplierCancelled = 93
}
