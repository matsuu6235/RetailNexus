using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Features.StoreRequests;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Api.Controllers;

[Route("api/store-requests")]
[Authorize]
public sealed class StoreRequestsController : BaseController
{
    private readonly IStoreRequestRepository _repo;
    private readonly IStoreRequestService _service;
    private readonly IValidator<CreateStoreRequestRequest> _createValidator;
    private readonly IValidator<UpdateStoreRequestRequest> _updateValidator;

    public StoreRequestsController(
        IStoreRequestRepository repo,
        IStoreRequestService service,
        IValidator<CreateStoreRequestRequest> createValidator,
        IValidator<UpdateStoreRequestRequest> updateValidator)
    {
        _repo = repo;
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── Request / Response records ──

    public sealed record CreateDetailRequest(
        Guid ProductId,
        int Quantity);

    public sealed record CreateStoreRequestRequest(
        Guid FromStoreId,
        Guid ToStoreId,
        DateTimeOffset RequestDate,
        DateTimeOffset? DesiredDeliveryDate,
        string? Note,
        List<CreateDetailRequest> Details);

    public sealed record UpdateDetailRequest(
        Guid? StoreRequestDetailId,
        Guid ProductId,
        int Quantity);

    public sealed record UpdateStoreRequestRequest(
        Guid FromStoreId,
        Guid ToStoreId,
        DateTimeOffset RequestDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        string? Note,
        List<UpdateDetailRequest> Details);

    public sealed record ChangeStatusRequest(StoreRequestStatus Status);

    public sealed record ChangeActivationRequest(bool IsActive);

    public sealed record StoreRequestListResponse(
        Guid StoreRequestId,
        string RequestNumber,
        Guid FromStoreId,
        string FromStoreName,
        Guid ToStoreId,
        string ToStoreName,
        DateTimeOffset RequestDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        DateTimeOffset? ShippedDate,
        DateTimeOffset? ReceivedDate,
        StoreRequestStatus Status,
        Guid? ApprovedBy,
        string? ApprovedByName,
        DateTimeOffset? ApprovedAt,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    public sealed record StoreRequestDetailResponse(
        Guid StoreRequestDetailId,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        int Quantity);

    public sealed record StoreRequestResponse(
        Guid StoreRequestId,
        string RequestNumber,
        Guid FromStoreId,
        string FromStoreName,
        Guid ToStoreId,
        string ToStoreName,
        DateTimeOffset RequestDate,
        DateTimeOffset? DesiredDeliveryDate,
        DateTimeOffset? ExpectedDeliveryDate,
        DateTimeOffset? ShippedDate,
        DateTimeOffset? ReceivedDate,
        StoreRequestStatus Status,
        string? Note,
        Guid? ApprovedBy,
        string? ApprovedByName,
        DateTimeOffset? ApprovedAt,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        Guid CreatedBy,
        Guid UpdatedBy,
        List<StoreRequestDetailResponse> Details);

    // ── Endpoints ──

    [HttpGet]
    [RequirePermission("store-requests.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? requestNumber,
        [FromQuery] Guid? fromStoreId,
        [FromQuery] Guid? toStoreId,
        [FromQuery] StoreRequestStatus? status,
        [FromQuery] DateTimeOffset? requestDateFrom,
        [FromQuery] DateTimeOffset? requestDateTo,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);
        var total = await _repo.CountAsync(requestNumber, fromStoreId, toStoreId, status, requestDateFrom, requestDateTo, isActive, ct);
        var items = await _repo.ListAsync(requestNumber, fromStoreId, toStoreId, status, requestDateFrom, requestDateTo, isActive, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(MapList)
        });
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("store-requests.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var request = await _repo.GetByIdWithDetailsAsync(id, ct);
        return request is null ? NotFound() : Ok(MapDetail(request));
    }

    [HttpGet("by-number/{requestNumber}")]
    [RequirePermission("store-requests.view")]
    public async Task<IActionResult> GetByRequestNumber(string requestNumber, CancellationToken ct)
    {
        var request = await _repo.GetByRequestNumberWithDetailsAsync(requestNumber, ct);
        return request is null ? NotFound() : Ok(MapDetail(request));
    }

    [HttpPost]
    [RequirePermission("store-requests.create")]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequestRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var details = req.Details.Select(d => new CreateStoreRequestDetailParam(d.ProductId, d.Quantity)).ToList();
        var storeRequest = await _service.CreateAsync(req.FromStoreId, req.ToStoreId, req.RequestDate, req.DesiredDeliveryDate, req.Note, details, actorUserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = storeRequest.StoreRequestId }, MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("store-requests.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreRequestRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateStoreRequestRequest>(req);
        ctx.RootContextData["RequestId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var details = req.Details.Select(d => new UpdateStoreRequestDetailParam(d.StoreRequestDetailId, d.ProductId, d.Quantity)).ToList();
        var storeRequest = await _service.UpdateAsync(id, req.FromStoreId, req.ToStoreId, req.RequestDate, req.DesiredDeliveryDate, req.ExpectedDeliveryDate, req.Note, details, actorUserId, ct);
        return Ok(MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}/submit")]
    [RequirePermission("store-requests.edit")]
    public async Task<IActionResult> SubmitForApproval(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var storeRequest = await _service.SubmitForApprovalAsync(id, actorUserId, ct);
        return Ok(MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}/approve")]
    [RequirePermission("store-requests.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var approverUserId))
            return Unauthorized();

        var storeRequest = await _service.ApproveAsync(id, approverUserId, ct);
        return Ok(MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}/reject")]
    [RequirePermission("store-requests.approve")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var storeRequest = await _service.RejectAsync(id, actorUserId, ct);
        return Ok(MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}/status")]
    [RequirePermission("store-requests.edit")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var storeRequest = await _service.ChangeStatusAsync(id, req.Status, actorUserId, ct);
        return Ok(MapDetail(storeRequest));
    }

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("store-requests.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var storeRequest = await _service.ChangeActivationAsync(id, req.IsActive, actorUserId, ct);
        return Ok(MapList(storeRequest));
    }

    // ── Helpers ──

    private static StoreRequestListResponse MapList(StoreRequest x)
        => new(
            x.StoreRequestId,
            x.RequestNumber,
            x.FromStoreId,
            x.FromStore?.StoreName ?? "",
            x.ToStoreId,
            x.ToStore?.StoreName ?? "",
            x.RequestDate,
            x.DesiredDeliveryDate,
            x.ExpectedDeliveryDate,
            x.ShippedDate,
            x.ReceivedDate,
            x.Status,
            x.ApprovedBy,
            x.Approver?.UserName,
            x.ApprovedAt,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt);

    private static StoreRequestResponse MapDetail(StoreRequest x)
        => new(
            x.StoreRequestId,
            x.RequestNumber,
            x.FromStoreId,
            x.FromStore?.StoreName ?? "",
            x.ToStoreId,
            x.ToStore?.StoreName ?? "",
            x.RequestDate,
            x.DesiredDeliveryDate,
            x.ExpectedDeliveryDate,
            x.ShippedDate,
            x.ReceivedDate,
            x.Status,
            x.Note,
            x.ApprovedBy,
            x.Approver?.UserName,
            x.ApprovedAt,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt,
            x.CreatedBy,
            x.UpdatedBy,
            x.Details.Select(d => new StoreRequestDetailResponse(
                d.StoreRequestDetailId,
                d.ProductId,
                d.Product?.ProductCode ?? "",
                d.Product?.ProductName ?? "",
                d.Quantity)).ToList());
}
