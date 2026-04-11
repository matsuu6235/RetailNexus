using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Api.Controllers;

[Route("api/inventory-transactions")]
[Authorize]
public sealed class InventoryTransactionsController : BaseController
{
    private readonly IInventoryTransactionRepository _repo;
    private readonly IInventoryService _service;
    private readonly IValidator<ManualTransactionRequest> _validator;

    public InventoryTransactionsController(
        IInventoryTransactionRepository repo,
        IInventoryService service,
        IValidator<ManualTransactionRequest> validator)
    {
        _repo = repo;
        _service = service;
        _validator = validator;
    }

    // ── Request / Response records ──

    public sealed record ManualTransactionRequest(
        Guid StoreId,
        Guid ProductId,
        InventoryTransactionType TransactionType,
        decimal QuantityChange,
        string? Note);

    public sealed record InventoryTransactionListResponse(
        Guid InventoryTransactionId,
        Guid StoreId,
        string StoreName,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        InventoryTransactionType TransactionType,
        decimal QuantityChange,
        decimal QuantityAfter,
        DateTimeOffset OccurredAt,
        string? ReferenceNumber,
        string? Note,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    // ── Endpoints ──

    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? productId,
        [FromQuery] InventoryTransactionType? transactionType,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);
        var total = await _repo.CountAsync(storeId, productId, transactionType, dateFrom, dateTo, ct);
        var items = await _repo.ListAsync(storeId, productId, transactionType, dateFrom, dateTo, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(MapResponse)
        });
    }

    [HttpPost("manual")]
    [RequirePermission("inventory.edit")]
    public async Task<IActionResult> CreateManualTransaction([FromBody] ManualTransactionRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var validation = await _validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var transaction = await _service.ApplyTransactionAsync(
            req.StoreId,
            req.ProductId,
            req.TransactionType,
            req.QuantityChange,
            DateTimeOffset.UtcNow,
            null,
            req.Note,
            actorUserId,
            ct);

        return Created($"api/inventory-transactions/{transaction.InventoryTransactionId}", MapResponse(transaction));
    }

    // ── Helpers ──

    private static InventoryTransactionListResponse MapResponse(InventoryTransaction x)
        => new(
            x.InventoryTransactionId,
            x.StoreId,
            x.Store?.StoreName ?? "",
            x.ProductId,
            x.Product?.ProductCode ?? "",
            x.Product?.ProductName ?? "",
            x.TransactionType,
            x.QuantityChange,
            x.QuantityAfter,
            x.OccurredAt,
            x.ReferenceNumber,
            x.Note,
            x.CreatedAt,
            x.CreatedBy);
}
