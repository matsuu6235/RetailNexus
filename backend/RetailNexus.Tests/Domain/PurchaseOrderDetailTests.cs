using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class PurchaseOrderDetailTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var detail = new PurchaseOrderDetail(_orderId, _productId, 10, 150.50m, _actorUserId);

        detail.PurchaseOrderId.Should().Be(_orderId);
        detail.ProductId.Should().Be(_productId);
        detail.Quantity.Should().Be(10);
        detail.UnitPrice.Should().Be(150.50m);
        detail.SubTotal.Should().Be(1505.00m);
        detail.CreatedBy.Should().Be(_actorUserId);
        detail.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Constructor_ShouldCalculateSubTotal()
    {
        var detail = new PurchaseOrderDetail(_orderId, _productId, 3, 200m, _actorUserId);

        detail.SubTotal.Should().Be(600m);
    }

    [Fact]
    public void Update_ShouldRecalculateSubTotal()
    {
        var detail = new PurchaseOrderDetail(_orderId, _productId, 10, 100m, _actorUserId);
        var updater = Guid.NewGuid();
        var before = detail.UpdatedAt;

        detail.Update(5, 300m, updater);

        detail.Quantity.Should().Be(5);
        detail.UnitPrice.Should().Be(300m);
        detail.SubTotal.Should().Be(1500m);
        detail.UpdatedBy.Should().Be(updater);
        detail.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldNotChangeCreatedBy()
    {
        var detail = new PurchaseOrderDetail(_orderId, _productId, 10, 100m, _actorUserId);
        var updater = Guid.NewGuid();

        detail.Update(5, 200m, updater);

        detail.CreatedBy.Should().Be(_actorUserId);
        detail.UpdatedBy.Should().Be(updater);
    }
}
