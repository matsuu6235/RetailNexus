using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Features.PurchaseOrders;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Api.Controllers;

[Route("api/purchase-orders")]
[Authorize]
public sealed class PurchaseOrdersController : BaseController
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly IPurchaseOrderService _service;
    private readonly IValidator<CreatePurchaseOrderRequest> _createValidator;
    private readonly IValidator<UpdatePurchaseOrderRequest> _updateValidator;

    public PurchaseOrdersController(
        IPurchaseOrderRepository repo,
        IPurchaseOrderService service,
        IValidator<CreatePurchaseOrderRequest> createValidator,
        IValidator<UpdatePurchaseOrderRequest> updateValidator)
    {
        _repo = repo;
        _service = service;
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

    [HttpGet("by-number/{orderNumber}")]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct)
    {
        var order = await _repo.GetByOrderNumberWithDetailsAsync(orderNumber, ct);
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

        var details = req.Details.Select(d => new CreatePurchaseOrderDetailParam(d.ProductId, d.Quantity, d.UnitPrice)).ToList();
        var order = await _service.CreateAsync(req.SupplierId, req.StoreId, req.OrderDate, req.DesiredDeliveryDate, req.Note, details, actorUserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.PurchaseOrderId }, MapDetail(order));
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

        var details = req.Details.Select(d => new UpdatePurchaseOrderDetailParam(d.PurchaseOrderDetailId, d.ProductId, d.Quantity, d.UnitPrice)).ToList();
        var order = await _service.UpdateAsync(id, req.SupplierId, req.StoreId, req.OrderDate, req.DesiredDeliveryDate, req.ExpectedDeliveryDate, req.Note, details, actorUserId, ct);
        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/submit")]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> SubmitForApproval(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _service.SubmitForApprovalAsync(id, actorUserId, ct);
        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/approve")]
    [RequirePermission("purchases.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var approverUserId))
            return Unauthorized();

        var order = await _service.ApproveAsync(id, approverUserId, ct);
        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/reject")]
    [RequirePermission("purchases.approve")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _service.RejectAsync(id, actorUserId, ct);
        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/status")]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _service.ChangeStatusAsync(id, req.Status, actorUserId, ct);
        return Ok(MapDetail(order));
    }

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("purchases.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var order = await _service.ChangeActivationAsync(id, req.IsActive, actorUserId, ct);
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
