using FluentAssertions;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Tests.Domain;

public class PurchaseOrderTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _storeId = Guid.NewGuid();

    private PurchaseOrder CreateOrder()
        => new("PO-000001", _supplierId, _storeId, DateTimeOffset.UtcNow, null, "テスト備考", _actorUserId);

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var order = CreateOrder();

        order.OrderNumber.Should().Be("PO-000001");
        order.SupplierId.Should().Be(_supplierId);
        order.StoreId.Should().Be(_storeId);
        order.Note.Should().Be("テスト備考");
        order.Status.Should().Be(PurchaseOrderStatus.Draft);
        order.IsActive.Should().BeTrue();
        order.TotalAmount.Should().Be(0);
        order.ApprovedBy.Should().BeNull();
        order.ApprovedAt.Should().BeNull();
        order.CreatedBy.Should().Be(_actorUserId);
        order.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Constructor_ShouldTrimOrderNumber()
    {
        var order = new PurchaseOrder("  PO-000001  ", _supplierId, _storeId, DateTimeOffset.UtcNow, null, null, _actorUserId);

        order.OrderNumber.Should().Be("PO-000001");
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmptyNoteToNull()
    {
        var order = new PurchaseOrder("PO-000001", _supplierId, _storeId, DateTimeOffset.UtcNow, null, "  ", _actorUserId);

        order.Note.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var order = CreateOrder();
        var newSupplierId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid();
        var updater = Guid.NewGuid();
        var before = order.UpdatedAt;

        order.Update(newSupplierId, newStoreId, DateTimeOffset.UtcNow, null, null, "更新備考", updater);

        order.SupplierId.Should().Be(newSupplierId);
        order.StoreId.Should().Be(newStoreId);
        order.Note.Should().Be("更新備考");
        order.UpdatedBy.Should().Be(updater);
        order.UpdatedAt.Should().BeOnOrAfter(before);
        order.CreatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void SubmitForApproval_ShouldChangeStatusToAwaitingApproval()
    {
        var order = CreateOrder();

        order.SubmitForApproval(_actorUserId);

        order.Status.Should().Be(PurchaseOrderStatus.AwaitingApproval);
    }

    [Fact]
    public void Approve_ShouldSetStatusAndApproverInfo()
    {
        var order = CreateOrder();
        var approverId = Guid.NewGuid();
        order.SubmitForApproval(_actorUserId);

        order.Approve(approverId);

        order.Status.Should().Be(PurchaseOrderStatus.Approved);
        order.ApprovedBy.Should().Be(approverId);
        order.ApprovedAt.Should().NotBeNull();
        order.UpdatedBy.Should().Be(approverId);
    }

    [Fact]
    public void Reject_ShouldResetStatusAndClearApproverInfo()
    {
        var order = CreateOrder();
        order.SubmitForApproval(_actorUserId);

        order.Reject(_actorUserId);

        order.Status.Should().Be(PurchaseOrderStatus.Draft);
        order.ApprovedBy.Should().BeNull();
        order.ApprovedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(PurchaseOrderStatus.AwaitingApproval)]
    [InlineData(PurchaseOrderStatus.Approved)]
    [InlineData(PurchaseOrderStatus.Shipped)]
    public void SubmitForApproval_ShouldThrow_WhenNotDraft(PurchaseOrderStatus initialStatus)
    {
        var order = CreateOrder();
        order.SetStatus(initialStatus, _actorUserId);

        var act = () => order.SubmitForApproval(_actorUserId);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("下書き状態のみ");
    }

    [Theory]
    [InlineData(PurchaseOrderStatus.Draft)]
    [InlineData(PurchaseOrderStatus.Approved)]
    [InlineData(PurchaseOrderStatus.Shipped)]
    public void Approve_ShouldThrow_WhenNotAwaitingApproval(PurchaseOrderStatus initialStatus)
    {
        var order = CreateOrder();
        if (initialStatus != PurchaseOrderStatus.Draft)
            order.SetStatus(initialStatus, _actorUserId);

        var act = () => order.Approve(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("承認待ち状態のみ");
    }

    [Theory]
    [InlineData(PurchaseOrderStatus.Draft)]
    [InlineData(PurchaseOrderStatus.Approved)]
    [InlineData(PurchaseOrderStatus.Shipped)]
    public void Reject_ShouldThrow_WhenNotAwaitingApproval(PurchaseOrderStatus initialStatus)
    {
        var order = CreateOrder();
        if (initialStatus != PurchaseOrderStatus.Draft)
            order.SetStatus(initialStatus, _actorUserId);

        var act = () => order.Reject(_actorUserId);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("承認待ち状態のみ");
    }

    [Fact]
    public void SetStatus_Received_ShouldSetReceivedDate()
    {
        var order = CreateOrder();

        order.SetStatus(PurchaseOrderStatus.Received, _actorUserId);

        order.Status.Should().Be(PurchaseOrderStatus.Received);
        order.ReceivedDate.Should().NotBeNull();
    }

    [Fact]
    public void SetStatus_Shipped_ShouldNotSetReceivedDate()
    {
        var order = CreateOrder();

        order.SetStatus(PurchaseOrderStatus.Shipped, _actorUserId);

        order.ReceivedDate.Should().BeNull();
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateAudit()
    {
        var order = CreateOrder();
        var updater = Guid.NewGuid();

        order.SetActivation(false, updater);

        order.IsActive.Should().BeFalse();
        order.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetDetails_ShouldCalculateTotalAmount()
    {
        var order = CreateOrder();
        var details = new[]
        {
            new PurchaseOrderDetail(order.PurchaseOrderId, Guid.NewGuid(), 10, 100m, _actorUserId),
            new PurchaseOrderDetail(order.PurchaseOrderId, Guid.NewGuid(), 5, 200m, _actorUserId),
        };

        order.SetDetails(details);

        order.TotalAmount.Should().Be(2000m); // 10*100 + 5*200
        order.Details.Should().HaveCount(2);
    }

    [Fact]
    public void RecalculateTotal_ShouldSumSubTotals()
    {
        var order = CreateOrder();
        var details = new[]
        {
            new PurchaseOrderDetail(order.PurchaseOrderId, Guid.NewGuid(), 3, 500m, _actorUserId),
        };
        order.SetDetails(details);

        order.RecalculateTotal();

        order.TotalAmount.Should().Be(1500m);
    }
}
