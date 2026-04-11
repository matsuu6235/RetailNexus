using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Features.StoreRequests;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Services;

public class StoreRequestService : IStoreRequestService
{
    private readonly IStoreRequestRepository _repo;
    private readonly IInventoryService _inventoryService;

    public StoreRequestService(IStoreRequestRepository repo, IInventoryService inventoryService)
    {
        _repo = repo;
        _inventoryService = inventoryService;
    }

    public async Task<StoreRequest> CreateAsync(Guid fromStoreId, Guid toStoreId, DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate, string? note,
        List<CreateStoreRequestDetailParam> details, Guid actorId, CancellationToken ct)
    {
        var maxNumber = await _repo.GetMaxRequestNumberAsync(ct);
        var requestNumber = CodeGenerator.NextRequestNumber(maxNumber);

        var storeRequest = new StoreRequest(requestNumber, fromStoreId, toStoreId, requestDate, desiredDeliveryDate, note, actorId);

        var detailEntities = details.Select(d =>
            new StoreRequestDetail(storeRequest.StoreRequestId, d.ProductId, d.Quantity, actorId));
        storeRequest.SetDetails(detailEntities);

        await _repo.AddAsync(storeRequest, ct);
        await _repo.SaveChangesAsync(ct);

        var created = await _repo.GetByIdWithDetailsAsync(storeRequest.StoreRequestId, ct);
        return created!;
    }

    public async Task<StoreRequest> UpdateAsync(Guid id, Guid fromStoreId, Guid toStoreId, DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate, DateTimeOffset? expectedDeliveryDate, string? note,
        List<UpdateStoreRequestDetailParam> details, Guid actorId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.Update(fromStoreId, toStoreId, requestDate, desiredDeliveryDate, expectedDeliveryDate, note, actorId);

        // 明細の個別更新
        var existingDetails = storeRequest.Details.ToDictionary(d => d.StoreRequestDetailId);
        var incomingIds = details
            .Where(d => d.DetailId.HasValue)
            .Select(d => d.DetailId!.Value)
            .ToHashSet();

        // 削除: リクエストに含まれていない既存行
        var toRemove = existingDetails.Values
            .Where(d => !incomingIds.Contains(d.StoreRequestDetailId))
            .ToList();
        foreach (var r in toRemove)
            storeRequest.Details.Remove(r);
        _repo.RemoveDetails(toRemove);

        // 更新: IDが一致する行
        foreach (var d in details.Where(d => d.DetailId.HasValue))
        {
            if (existingDetails.TryGetValue(d.DetailId!.Value, out var existing))
            {
                existing.Update(d.Quantity, actorId);
            }
        }

        // 追加: IDがnullの行
        foreach (var d in details.Where(d => !d.DetailId.HasValue))
        {
            var newDetail = new StoreRequestDetail(storeRequest.StoreRequestId, d.ProductId, d.Quantity, actorId);
            _repo.AddDetail(newDetail);
        }

        await _repo.SaveChangesAsync(ct);

        var updated = await _repo.GetByIdWithDetailsAsync(id, ct);
        return updated!;
    }

    public async Task<StoreRequest> SubmitForApprovalAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.SubmitForApproval(actorId);
        await _repo.SaveChangesAsync(ct);

        return storeRequest;
    }

    public async Task<StoreRequest> ApproveAsync(Guid id, Guid approverId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.Approve(approverId);
        await _repo.SaveChangesAsync(ct);

        return storeRequest;
    }

    public async Task<StoreRequest> RejectAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.Reject(actorId);
        await _repo.SaveChangesAsync(ct);

        return storeRequest;
    }

    public async Task<StoreRequest> ChangeStatusAsync(Guid id, StoreRequestStatus status, Guid actorId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.SetStatus(status, actorId);
        await _repo.SaveChangesAsync(ct);

        // 出荷済 → 依頼先（ToStore）から出庫
        if (status == StoreRequestStatus.Shipped)
        {
            foreach (var detail in storeRequest.Details)
            {
                await _inventoryService.ApplyTransactionAsync(
                    storeRequest.ToStoreId,
                    detail.ProductId,
                    InventoryTransactionType.ShipmentOut,
                    -detail.Quantity,
                    DateTimeOffset.UtcNow,
                    storeRequest.RequestNumber,
                    null,
                    actorId,
                    ct);
            }
        }

        // 入荷済 → 依頼元（FromStore）に入庫
        if (status == StoreRequestStatus.Received)
        {
            foreach (var detail in storeRequest.Details)
            {
                await _inventoryService.ApplyTransactionAsync(
                    storeRequest.FromStoreId,
                    detail.ProductId,
                    InventoryTransactionType.ShipmentIn,
                    detail.Quantity,
                    DateTimeOffset.UtcNow,
                    storeRequest.RequestNumber,
                    null,
                    actorId,
                    ct);
            }
        }

        return storeRequest;
    }

    public async Task<StoreRequest> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var storeRequest = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreRequest", id);

        storeRequest.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return storeRequest;
    }
}
