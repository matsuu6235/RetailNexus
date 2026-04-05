using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class SuppliersController : BaseController
{
    private readonly ISupplierRepository _repo;
    private readonly ISupplierService _service;
    private readonly IValidator<CreateSupplierRequest> _createValidator;
    private readonly IValidator<UpdateSupplierRequest> _updateValidator;

    public SuppliersController(
        ISupplierRepository repo,
        ISupplierService service,
        IValidator<CreateSupplierRequest> createValidator,
        IValidator<UpdateSupplierRequest> updateValidator)
    {
        _repo = repo;
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public sealed record CreateSupplierRequest(
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive = true) : ISupplierRequest;

    public sealed record UpdateSupplierRequest(
        string SupplierName,
        string? PhoneNumber,
        string? Email) : ISupplierRequest;

    public sealed record SupplierNewResponse(
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive);

    public sealed record SupplierResponse(
        Guid SupplierId,
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        Guid CreatedBy,
        Guid UpdatedBy);

    [HttpGet]
    [RequirePermission("suppliers.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? supplierCode,
        [FromQuery] string? supplierName,
        [FromQuery] string? phoneNumber,
        [FromQuery] string? email,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);
        var total = await _repo.CountAsync(supplierCode, supplierName, phoneNumber, email, isActive, ct);
        var items = await _repo.ListAsync(supplierCode, supplierName, phoneNumber, email, isActive, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(Map)
        });
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("suppliers.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct);
        return supplier is null ? NotFound() : Ok(Map(supplier));
    }

    [HttpGet("new")]
    [RequirePermission("suppliers.create")]
    public IActionResult New()
        => Ok(new SupplierNewResponse(string.Empty, string.Empty, null, null, true));

    [HttpPost]
    [RequirePermission("suppliers.create")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var supplier = await _service.CreateAsync(req.SupplierName, req.PhoneNumber, req.Email, req.IsActive, actorUserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, Map(supplier));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("suppliers.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateSupplierRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var supplier = await _service.UpdateAsync(id, req.SupplierName, req.PhoneNumber, req.Email, actorUserId, ct);
        return Ok(Map(supplier));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("suppliers.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var supplier = await _service.ChangeActivationAsync(id, req.IsActive, actorUserId, ct);
        return Ok(Map(supplier));
    }

    private static SupplierResponse Map(Supplier x)
        => new(
            x.SupplierId,
            x.SupplierCode,
            x.SupplierName,
            x.PhoneNumber,
            x.Email,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt,
            x.CreatedBy,
            x.UpdatedBy);
}
