using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Features.PurchaseOrders;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repo;

    public PurchaseOrderService(IPurchaseOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<PurchaseOrder> CreateAsync(Guid supplierId, Guid storeId, DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate, string? note,
        List<CreatePurchaseOrderDetailParam> details, Guid actorId, CancellationToken ct)
    {
        var maxNumber = await _repo.GetMaxOrderNumberAsync(ct);
        var orderNumber = CodeGenerator.NextOrderNumber(maxNumber);

        var order = new PurchaseOrder(orderNumber, supplierId, storeId, orderDate, desiredDeliveryDate, note, actorId);

        var detailEntities = details.Select(d =>
            new PurchaseOrderDetail(order.PurchaseOrderId, d.ProductId, d.Quantity, d.UnitPrice, actorId));
        order.SetDetails(detailEntities);

        await _repo.AddAsync(order, ct);
        await _repo.SaveChangesAsync(ct);

        var created = await _repo.GetByIdWithDetailsAsync(order.PurchaseOrderId, ct);
        return created!;
    }

    public async Task<PurchaseOrder> UpdateAsync(Guid id, Guid supplierId, Guid storeId, DateTimeOffset orderDate,
        DateTimeOffset? desiredDeliveryDate, DateTimeOffset? expectedDeliveryDate, string? note,
        List<UpdatePurchaseOrderDetailParam> details, Guid actorId, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.Update(supplierId, storeId, orderDate, desiredDeliveryDate, expectedDeliveryDate, note, actorId);

        // 明細の個別更新
        var existingDetails = order.Details.ToDictionary(d => d.PurchaseOrderDetailId);
        var incomingIds = details
            .Where(d => d.DetailId.HasValue)
            .Select(d => d.DetailId!.Value)
            .ToHashSet();

        // 削除: リクエストに含まれていない既存行
        var toRemove = existingDetails.Values
            .Where(d => !incomingIds.Contains(d.PurchaseOrderDetailId))
            .ToList();
        foreach (var r in toRemove)
            order.Details.Remove(r);
        _repo.RemoveDetails(toRemove);

        // 更新: IDが一致する行
        foreach (var d in details.Where(d => d.DetailId.HasValue))
        {
            if (existingDetails.TryGetValue(d.DetailId!.Value, out var existing))
            {
                existing.Update(d.Quantity, d.UnitPrice, actorId);
            }
        }

        // 追加: IDがnullの行
        foreach (var d in details.Where(d => !d.DetailId.HasValue))
        {
            var newDetail = new PurchaseOrderDetail(order.PurchaseOrderId, d.ProductId, d.Quantity, d.UnitPrice, actorId);
            _repo.AddDetail(newDetail);
        }

        order.RecalculateTotal();
        await _repo.SaveChangesAsync(ct);

        var updated = await _repo.GetByIdWithDetailsAsync(id, ct);
        return updated!;
    }

    public async Task<PurchaseOrder> SubmitForApprovalAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.SubmitForApproval(actorId);
        await _repo.SaveChangesAsync(ct);

        return order;
    }

    public async Task<PurchaseOrder> ApproveAsync(Guid id, Guid approverId, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.Approve(approverId);
        await _repo.SaveChangesAsync(ct);

        return order;
    }

    public async Task<PurchaseOrder> RejectAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.Reject(actorId);
        await _repo.SaveChangesAsync(ct);

        return order;
    }

    public async Task<PurchaseOrder> ChangeStatusAsync(Guid id, PurchaseOrderStatus status, Guid actorId, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.SetStatus(status, actorId);
        await _repo.SaveChangesAsync(ct);

        return order;
    }

    public async Task<PurchaseOrder> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", id);

        order.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return order;
    }
}
