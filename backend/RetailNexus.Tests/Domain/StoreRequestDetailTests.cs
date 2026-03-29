using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class StoreRequestDetailTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _requestId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var detail = new StoreRequestDetail(_requestId, _productId, 10, _actorUserId);

        detail.StoreRequestId.Should().Be(_requestId);
        detail.ProductId.Should().Be(_productId);
        detail.Quantity.Should().Be(10);
        detail.CreatedBy.Should().Be(_actorUserId);
        detail.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Update_ShouldModifyQuantityAndAudit()
    {
        var detail = new StoreRequestDetail(_requestId, _productId, 10, _actorUserId);
        var updater = Guid.NewGuid();
        var before = detail.UpdatedAt;

        detail.Update(25, updater);

        detail.Quantity.Should().Be(25);
        detail.UpdatedBy.Should().Be(updater);
        detail.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldNotChangeCreatedBy()
    {
        var detail = new StoreRequestDetail(_requestId, _productId, 10, _actorUserId);
        var updater = Guid.NewGuid();

        detail.Update(20, updater);

        detail.CreatedBy.Should().Be(_actorUserId);
        detail.UpdatedBy.Should().Be(updater);
    }
}
