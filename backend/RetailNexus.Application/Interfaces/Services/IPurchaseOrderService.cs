using RetailNexus.Application.Features.PurchaseOrders;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrder> CreateAsync(Guid supplierId, Guid storeId, DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate, string? note,
        List<CreatePurchaseOrderDetailParam> details, Guid actorId, CancellationToken ct);

    Task<PurchaseOrder> UpdateAsync(Guid id, Guid supplierId, Guid storeId, DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate, DateTimeOffset? expectedDeliveryDate, string? note,
        List<UpdatePurchaseOrderDetailParam> details, Guid actorId, CancellationToken ct);

    Task<PurchaseOrder> SubmitForApprovalAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<PurchaseOrder> ApproveAsync(Guid id, Guid approverId, CancellationToken ct);
    Task<PurchaseOrder> RejectAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<PurchaseOrder> ChangeStatusAsync(Guid id, PurchaseOrderStatus status, Guid actorId, CancellationToken ct);
    Task<PurchaseOrder> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
