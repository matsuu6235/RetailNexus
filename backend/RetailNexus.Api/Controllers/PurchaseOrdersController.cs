using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Api.Controllers;

[Route("api/purchase-orders")]
[Authorize]
public sealed class PurchaseOrdersController : BaseController
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly IValidator<CreatePurchaseOrderRequest> _createValidator;
    private readonly IValidator<UpdatePurchaseOrderRequest> _updateValidator;

    public PurchaseOrdersController(
        IPurchaseOrderRepository repo,
        IValidator<CreatePurchaseOrderRequest> createValidator,
        IValidator<UpdatePurchaseOrderRequest> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── Request / Response records ──

    public sealed record CreateDetailRequest(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice);

    public sealed record CreatePurchaseOrderRequest(
        Guid SupplierId,
        Guid StoreId,
        DateTimeOffset OrderDate,
        DateTimeOffset? DesiredDeliveryDate,
        string? Note,
        List<CreateDetailRequest> Details);

    public sealed record UpdateDetailRequest(
        Guid? PurchaseOrderDetailId,
        Guid ProductId,
        int Quantity,
        decimal UnitPrice);

    public sealed record UpdatePurchaseOrderRequest(
        Guid SupplierId,
        Guid StoreId,
        DateTimeOffset OrderDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        string? Note,
        List<UpdateDetailRequest> Details);

    public sealed record ChangeStatusRequest(PurchaseOrderStatus Status);

    public sealed record ChangeActivationRequest(bool IsActive);

    public sealed record PurchaseOrderListResponse(
        Guid PurchaseOrderId,
        string OrderNumber,
        Guid SupplierId,
        string SupplierName,
        Guid StoreId,
        string StoreName,
        DateTimeOffset OrderDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        DateTimeOffset? ReceivedDate,
        PurchaseOrderStatus Status,
        decimal TotalAmount,
        Guid? ApprovedBy,
        string? ApprovedByName,
        DateTimeOffset? ApprovedAt,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    public sealed record PurchaseOrderDetailResponse(
        Guid PurchaseOrderDetailId,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal SubTotal);

    public sealed record PurchaseOrderResponse(
        Guid PurchaseOrderId,
        string OrderNumber,
        Guid SupplierId,
        string SupplierName,
        Guid StoreId,
        string StoreName,
        DateTimeOffset OrderDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        DateTimeOffset? ReceivedDate,
        PurchaseOrderStatus Status,
        decimal TotalAmount,
        string? Note,
        Guid? ApprovedBy,
        string? ApprovedByName,
        DateTimeOffset? ApprovedAt,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        Guid CreatedBy,
        Guid UpdatedBy,
        List<PurchaseOrderDetailResponse> Details);

    // ── Endpoints ──

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? orderNumber,
        [FromQuery] Guid? supplierId,
        [FromQuery] Guid? storeId,
        [FromQuery] PurchaseOrderStatus? status,
        [FromQuery] DateTimeOffset? orderDateFrom,
        [FromQuery] DateTimeOffset? orderDateTo,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);
        var total = await _repo.CountAsync(orderNumber, supplierId, storeId, status, orderDateFrom, orderDateTo, isActive, ct);
        var items = await _repo.ListAsync(orderNumber, supplierId, storeId, status, orderDateFrom, orderDateTo, isActive, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(MapList)
        });
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        return order is null ? NotFound() : Ok(MapDetail(order));
    }

    [HttpPost]
    [RequirePermission("purchases.create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var maxNumber = await _repo.GetMaxOrderNumberAsync(ct);
        var orderNumber = CodeGenerator.NextOrderNumber(maxNumber);

        var order = new PurchaseOrder(
            orderNumber,
            req.SupplierId,
            req.StoreId,
            req.OrderDate,
            req.DesiredDeliveryDate,
            req.Note,
            actorUserId);

        var details = req.Details.Select(d =>
            new PurchaseOrderDetail(order.PurchaseOrderId, d.ProductId, d.Quantity, d.UnitPrice, actorUserId));
        order.SetDetails(details);

        await _repo.AddAsync(order, ct);
        await _repo.SaveChangesAsync(ct);

        var created = await _repo.GetByIdWithDetailsAsync(order.PurchaseOrderId, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.PurchaseOrderId }, MapDetail(created!));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdatePurchaseOrderRequest>(req);
        ctx.RootContextData["OrderId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (order is null)
            return NotFound();

        order.Update(
            req.SupplierId,
            req.StoreId,
            req.OrderDate,
            req.DesiredDeliveryDate,
            req.ExpectedDeliveryDate,
            req.Note,
            actorUserId);

        // 明細の個別更新
        var existingDetails = order.Details.ToDictionary(d => d.PurchaseOrderDetailId);
        var incomingIds = req.Details
            .Where(d => d.PurchaseOrderDetailId.HasValue)
            .Select(d => d.PurchaseOrderDetailId!.Value)
            .ToHashSet();

        // 削除: リクエストに含まれていない既存行
        var toRemove = existingDetails.Values
            .Where(d => !incomingIds.Contains(d.PurchaseOrderDetailId))
            .ToList();
        foreach (var r in toRemove)
            order.Details.Remove(r);
        _repo.RemoveDetails(toRemove);

        // 更新: IDが一致する行
        foreach (var d in req.Details.Where(d => d.PurchaseOrderDetailId.HasValue))
        {
            if (existingDetails.TryGetValue(d.PurchaseOrderDetailId!.Value, out var existing))
            {
                existing.Update(d.Quantity, d.UnitPrice, actorUserId);
            }
        }

        // 追加: IDがnullの行（DbSet.Add で明示的に Added 状態にする）
        foreach (var d in req.Details.Where(d => !d.PurchaseOrderDetailId.HasValue))
        {
            var newDetail = new PurchaseOrderDetail(
                order.PurchaseOrderId, d.ProductId, d.Quantity, d.UnitPrice, actorUserId);
            _repo.AddDetail(newDetail);
        }

        order.RecalculateTotal();
        await _repo.SaveChangesAsync(ct);

        var updated = await _repo.GetByIdWithDetailsAsync(id, ct);
        return Ok(MapDetail(updated!));
    }

    [HttpPut("{id:guid}/submit")]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> SubmitForApproval(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (order is null)
            return NotFound();

        order.SubmitForApproval(actorUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/approve")]
    [RequirePermission("purchases.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var approverUserId))
            return Unauthorized();

        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (order is null)
            return NotFound();

        order.Approve(approverUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/reject")]
    [RequirePermission("purchases.approve")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (order is null)
            return NotFound();

        order.Reject(actorUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/status")]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (order is null)
            return NotFound();

        order.SetStatus(req.Status, actorUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("purchases.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _repo.GetByIdAsync(id, ct);
        if (order is null)
            return NotFound();

        order.SetActivation(req.IsActive, actorUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(MapList(order));
    }

    // ── Helpers ──

    private static PurchaseOrderListResponse MapList(PurchaseOrder x)
        => new(
            x.PurchaseOrderId,
            x.OrderNumber,
            x.SupplierId,
            x.Supplier?.SupplierName ?? "",
            x.StoreId,
            x.Store?.StoreName ?? "",
            x.OrderDate,
            x.DesiredDeliveryDate,
            x.ExpectedDeliveryDate,
            x.ReceivedDate,
            x.Status,
            x.TotalAmount,
            x.ApprovedBy,
            x.Approver?.UserName,
            x.ApprovedAt,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt);

    private static PurchaseOrderResponse MapDetail(PurchaseOrder x)
        => new(
            x.PurchaseOrderId,
            x.OrderNumber,
            x.SupplierId,
            x.Supplier?.SupplierName ?? "",
            x.StoreId,
            x.Store?.StoreName ?? "",
            x.OrderDate,
            x.DesiredDeliveryDate,
            x.ExpectedDeliveryDate,
            x.ReceivedDate,
            x.Status,
            x.TotalAmount,
            x.Note,
            x.ApprovedBy,
            x.Approver?.UserName,
            x.ApprovedAt,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt,
            x.CreatedBy,
            x.UpdatedBy,
            x.Details.Select(d => new PurchaseOrderDetailResponse(
                d.PurchaseOrderDetailId,
                d.ProductId,
                d.Product?.ProductCode ?? "",
                d.Product?.ProductName ?? "",
                d.Quantity,
                d.UnitPrice,
                d.SubTotal)).ToList());
}
