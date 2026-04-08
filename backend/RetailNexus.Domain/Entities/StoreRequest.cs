using RetailNexus.Domain.Enums;

namespace RetailNexus.Domain.Entities;

public class StoreRequest
{
    public Guid StoreRequestId { get; private set; } = Guid.NewGuid();
    public string RequestNumber { get; private set; } = string.Empty;
    public Guid FromStoreId { get; private set; }
    public Guid ToStoreId { get; private set; }
    public DateTimeOffset RequestDate { get; private set; }
    public DateTimeOffset? DesiredDeliveryDate { get; private set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; private set; }
    public DateTimeOffset? ShippedDate { get; private set; }
    public DateTimeOffset? ReceivedDate { get; private set; }
    public StoreRequestStatus Status { get; private set; } = StoreRequestStatus.Draft;
    public string? Note { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public Store? FromStore { get; private set; }
    public Store? ToStore { get; private set; }
    public User? Approver { get; private set; }
    public ICollection<StoreRequestDetail> Details { get; private set; } = new List<StoreRequestDetail>();

    private StoreRequest()
    {
    }

    public StoreRequest(
        string requestNumber,
        Guid fromStoreId,
        Guid toStoreId,
        DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate,
        string? note,
        Guid actorUserId)
    {
        RequestNumber = requestNumber;
        FromStoreId = fromStoreId;
        ToStoreId = toStoreId;
        RequestDate = requestDate;
        DesiredDeliveryDate = desiredDeliveryDate;
        Note = NormalizeOptional(note);
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(
        Guid fromStoreId,
        Guid toStoreId,
        DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate,
        DateTimeOffset? expectedDeliveryDate,
        string? note,
        Guid actorUserId)
    {
        FromStoreId = fromStoreId;
        ToStoreId = toStoreId;
        RequestDate = requestDate;
        DesiredDeliveryDate = desiredDeliveryDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Note = NormalizeOptional(note);
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SubmitForApproval(Guid actorUserId)
    {
        if (Status != StoreRequestStatus.Draft)
            throw new InvalidOperationException($"承認申請は下書き状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = StoreRequestStatus.AwaitingApproval;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(Guid approverUserId)
    {
        if (Status != StoreRequestStatus.AwaitingApproval)
            throw new InvalidOperationException($"承認は承認待ち状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = StoreRequestStatus.Approved;
        ApprovedBy = approverUserId;
        ApprovedAt = DateTimeOffset.UtcNow;
        UpdatedBy = approverUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid actorUserId)
    {
        if (Status != StoreRequestStatus.AwaitingApproval)
            throw new InvalidOperationException($"差戻しは承認待ち状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = StoreRequestStatus.Draft;
        ApprovedBy = null;
        ApprovedAt = null;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(StoreRequestStatus status, Guid actorUserId)
    {
        Status = status;
        if (status == StoreRequestStatus.Shipped)
            ShippedDate = DateTimeOffset.UtcNow;
        if (status == StoreRequestStatus.Received)
            ReceivedDate = DateTimeOffset.UtcNow;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActivation(bool isActive, Guid actorUserId)
    {
        IsActive = isActive;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDetails(IEnumerable<StoreRequestDetail> details)
    {
        Details = details.ToList();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
