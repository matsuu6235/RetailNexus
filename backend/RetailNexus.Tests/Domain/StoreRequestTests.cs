using FluentAssertions;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Tests.Domain;

public class StoreRequestTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _fromStoreId = Guid.NewGuid();
    private readonly Guid _toStoreId = Guid.NewGuid();

    private StoreRequest CreateRequest()
        => new("SR-000001", _fromStoreId, _toStoreId, DateTimeOffset.UtcNow, null, "テスト備考", _actorUserId);

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var request = CreateRequest();

        request.RequestNumber.Should().Be("SR-000001");
        request.FromStoreId.Should().Be(_fromStoreId);
        request.ToStoreId.Should().Be(_toStoreId);
        request.Note.Should().Be("テスト備考");
        request.Status.Should().Be(StoreRequestStatus.Draft);
        request.IsActive.Should().BeTrue();
        request.ApprovedBy.Should().BeNull();
        request.ApprovedAt.Should().BeNull();
        request.ShippedDate.Should().BeNull();
        request.ReceivedDate.Should().BeNull();
        request.CreatedBy.Should().Be(_actorUserId);
        request.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmptyNoteToNull()
    {
        var request = new StoreRequest("SR-000001", _fromStoreId, _toStoreId, DateTimeOffset.UtcNow, null, "   ", _actorUserId);

        request.Note.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var request = CreateRequest();
        var newFromStoreId = Guid.NewGuid();
        var newToStoreId = Guid.NewGuid();
        var updater = Guid.NewGuid();

        request.Update(newFromStoreId, newToStoreId, DateTimeOffset.UtcNow, null, null, "更新備考", updater);

        request.FromStoreId.Should().Be(newFromStoreId);
        request.ToStoreId.Should().Be(newToStoreId);
        request.Note.Should().Be("更新備考");
        request.UpdatedBy.Should().Be(updater);
        request.CreatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void SubmitForApproval_ShouldChangeStatusToAwaitingApproval()
    {
        var request = CreateRequest();

        request.SubmitForApproval(_actorUserId);

        request.Status.Should().Be(StoreRequestStatus.AwaitingApproval);
    }

    [Fact]
    public void Approve_ShouldSetStatusAndApproverInfo()
    {
        var request = CreateRequest();
        var approverId = Guid.NewGuid();
        request.SubmitForApproval(_actorUserId);

        request.Approve(approverId);

        request.Status.Should().Be(StoreRequestStatus.Approved);
        request.ApprovedBy.Should().Be(approverId);
        request.ApprovedAt.Should().NotBeNull();
        request.UpdatedBy.Should().Be(approverId);
    }

    [Fact]
    public void Reject_ShouldResetStatusAndClearApproverInfo()
    {
        var request = CreateRequest();
        request.SubmitForApproval(_actorUserId);

        request.Reject(_actorUserId);

        request.Status.Should().Be(StoreRequestStatus.Draft);
        request.ApprovedBy.Should().BeNull();
        request.ApprovedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(StoreRequestStatus.AwaitingApproval)]
    [InlineData(StoreRequestStatus.Approved)]
    [InlineData(StoreRequestStatus.Shipped)]
    public void SubmitForApproval_ShouldThrow_WhenNotDraft(StoreRequestStatus initialStatus)
    {
        var request = CreateRequest();
        request.SetStatus(initialStatus, _actorUserId);

        var act = () => request.SubmitForApproval(_actorUserId);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("下書き状態のみ");
    }

    [Theory]
    [InlineData(StoreRequestStatus.Draft)]
    [InlineData(StoreRequestStatus.Approved)]
    [InlineData(StoreRequestStatus.Shipped)]
    public void Approve_ShouldThrow_WhenNotAwaitingApproval(StoreRequestStatus initialStatus)
    {
        var request = CreateRequest();
        if (initialStatus != StoreRequestStatus.Draft)
            request.SetStatus(initialStatus, _actorUserId);

        var act = () => request.Approve(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("承認待ち状態のみ");
    }

    [Theory]
    [InlineData(StoreRequestStatus.Draft)]
    [InlineData(StoreRequestStatus.Approved)]
    [InlineData(StoreRequestStatus.Shipped)]
    public void Reject_ShouldThrow_WhenNotAwaitingApproval(StoreRequestStatus initialStatus)
    {
        var request = CreateRequest();
        if (initialStatus != StoreRequestStatus.Draft)
            request.SetStatus(initialStatus, _actorUserId);

        var act = () => request.Reject(_actorUserId);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("承認待ち状態のみ");
    }

    [Fact]
    public void SetStatus_Shipped_ShouldSetShippedDate()
    {
        var request = CreateRequest();

        request.SetStatus(StoreRequestStatus.Shipped, _actorUserId);

        request.Status.Should().Be(StoreRequestStatus.Shipped);
        request.ShippedDate.Should().NotBeNull();
        request.ReceivedDate.Should().BeNull();
    }

    [Fact]
    public void SetStatus_Received_ShouldSetReceivedDate()
    {
        var request = CreateRequest();

        request.SetStatus(StoreRequestStatus.Received, _actorUserId);

        request.Status.Should().Be(StoreRequestStatus.Received);
        request.ReceivedDate.Should().NotBeNull();
    }

    [Fact]
    public void SetStatus_Preparing_ShouldNotSetDates()
    {
        var request = CreateRequest();

        request.SetStatus(StoreRequestStatus.Preparing, _actorUserId);

        request.ShippedDate.Should().BeNull();
        request.ReceivedDate.Should().BeNull();
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateAudit()
    {
        var request = CreateRequest();
        var updater = Guid.NewGuid();

        request.SetActivation(false, updater);

        request.IsActive.Should().BeFalse();
        request.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetDetails_ShouldSetDetailsCollection()
    {
        var request = CreateRequest();
        var details = new[]
        {
            new StoreRequestDetail(request.StoreRequestId, Guid.NewGuid(), 10, _actorUserId),
            new StoreRequestDetail(request.StoreRequestId, Guid.NewGuid(), 5, _actorUserId),
        };

        request.SetDetails(details);

        request.Details.Should().HaveCount(2);
    }
}
