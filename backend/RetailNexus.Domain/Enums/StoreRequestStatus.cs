namespace RetailNexus.Domain.Enums;

public enum StoreRequestStatus
{
    Draft = 0,
    AwaitingApproval = 1,
    Approved = 2,
    Confirmed = 3,
    Preparing = 4,
    Shipped = 5,
    Received = 6,
    CancelRequested = 91,
    Cancelled = 92,
    Rejected = 93
}
