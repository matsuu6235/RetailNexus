using RetailNexus.Domain.Enums;

namespace RetailNexus.Domain.Entities;


public class PurchaseOrder
{
    public Guid PurchaseOrderId { get; private set; } = Guid.NewGuid();
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid StoreId { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public DateTimeOffset? DesiredDeliveryDate { get; private set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; private set; }
    public DateTimeOffset? ReceivedDate { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public Supplier? Supplier { get; private set; }
    public Store? Store { get; private set; }
    public User? Approver { get; private set; }
    public ICollection<PurchaseOrderDetail> Details { get; private set; } = new List<PurchaseOrderDetail>();

    private PurchaseOrder()
    {
    }

    public PurchaseOrder(
        string orderNumber,
        Guid supplierId,
        Guid storeId,
        DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate,
        string? note,
        Guid actorUserId)
    {
        OrderNumber = orderNumber.Trim();
        SupplierId = supplierId;
        StoreId = storeId;
        OrderDate = orderDate;
        DesiredDeliveryDate = desiredDeliveryDate;
        Note = NormalizeOptional(note);
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(
        Guid supplierId,
        Guid storeId,
        DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate,
        DateTimeOffset? expectedDeliveryDate,
        string? note,
        Guid actorUserId)
    {
        SupplierId = supplierId;
        StoreId = storeId;
        OrderDate = orderDate;
        DesiredDeliveryDate = desiredDeliveryDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Note = NormalizeOptional(note);
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SubmitForApproval(Guid actorUserId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"承認申請は下書き状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = PurchaseOrderStatus.AwaitingApproval;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(Guid approverUserId)
    {
        if (Status != PurchaseOrderStatus.AwaitingApproval)
            throw new InvalidOperationException($"承認は承認待ち状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = PurchaseOrderStatus.Approved;
        ApprovedBy = approverUserId;
        ApprovedAt = DateTimeOffset.UtcNow;
        UpdatedBy = approverUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid actorUserId)
    {
        if (Status != PurchaseOrderStatus.AwaitingApproval)
            throw new InvalidOperationException($"差戻しは承認待ち状態のみ可能です。現在のステータス: {Status.ToDisplayName()}");

        Status = PurchaseOrderStatus.Draft;
        ApprovedBy = null;
        ApprovedAt = null;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(PurchaseOrderStatus status, Guid actorUserId)
    {
        Status = status;
        if (status == PurchaseOrderStatus.Received)
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

    public void RecalculateTotal()
    {
        TotalAmount = Details.Sum(d => d.SubTotal);
    }

    public void SetDetails(IEnumerable<PurchaseOrderDetail> details)
    {
        Details = details.ToList();
        RecalculateTotal();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
