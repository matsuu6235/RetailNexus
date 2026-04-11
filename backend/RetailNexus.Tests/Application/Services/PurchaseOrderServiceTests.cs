using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Features.PurchaseOrders;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Tests.Application.Services;

public class PurchaseOrderServiceTests
{
    private readonly Mock<IPurchaseOrderRepository> _repoMock = new();
    private readonly Mock<IInventoryService> _inventoryServiceMock = new();
    private readonly PurchaseOrderService _service;

    public PurchaseOrderServiceTests()
    {
        _service = new PurchaseOrderService(_repoMock.Object, _inventoryServiceMock.Object);
    }

    private static PurchaseOrder CreateDraftOrder(Guid actorId)
    {
        return new PurchaseOrder("PO-000001", Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), "テスト備考", actorId);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateOrderNumber_CreateWithDetails()
    {
        var actorId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var details = new List<CreatePurchaseOrderDetailParam>
        {
            new(productId, 10, 500m)
        };

        _repoMock.Setup(r => r.GetMaxOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PO-000001");
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var order = new PurchaseOrder("PO-000002", Guid.NewGuid(), Guid.NewGuid(),
                    DateTimeOffset.UtcNow, null, null, actorId);
                return order;
            });

        var result = await _service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), "テスト備考", details, actorId, CancellationToken.None);

        result.OrderNumber.Should().Be("PO-000002");
        _repoMock.Verify(r => r.AddAsync(It.Is<PurchaseOrder>(o => o.OrderNumber == "PO-000002"), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var order = CreateDraftOrder(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(order.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.SubmitForApprovalAsync(order.PurchaseOrderId, actorId, CancellationToken.None);

        result.Status.Should().Be(PurchaseOrderStatus.AwaitingApproval);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var order = CreateDraftOrder(actorId);
        order.SubmitForApproval(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(order.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.ApproveAsync(order.PurchaseOrderId, approverId, CancellationToken.None);

        result.Status.Should().Be(PurchaseOrderStatus.Approved);
        result.ApprovedBy.Should().Be(approverId);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var order = CreateDraftOrder(actorId);
        order.SubmitForApproval(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(order.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.RejectAsync(order.PurchaseOrderId, actorId, CancellationToken.None);

        result.Status.Should().Be(PurchaseOrderStatus.Draft);
        result.ApprovedBy.Should().BeNull();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var order = CreateDraftOrder(actorId);

        _repoMock.Setup(r => r.GetByIdAsync(order.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.ChangeActivationAsync(order.PurchaseOrderId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
