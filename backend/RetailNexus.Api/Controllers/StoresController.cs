using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Api.Contracts;
using RetailNexus.Api.Controllers;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class StoresController : BaseController
{
    private readonly IStoreRepository _storeRepo;
    private readonly IValidator<CreateStoreRequest> _createValidator;
    private readonly IValidator<UpdateStoreRequest> _updateValidator;

    public StoresController(
        IStoreRepository storeRepo,
        IValidator<CreateStoreRequest> createValidator,
        IValidator<UpdateStoreRequest> updateValidator)
    {
        _storeRepo = storeRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public sealed record CreateStoreRequest(string StoreName, Guid AreaId, Guid StoreTypeId, bool IsActive = true) : IStoreRequest;
    public sealed record UpdateStoreRequest(string StoreName, Guid AreaId, Guid StoreTypeId) : IStoreRequest;
    public sealed record StoreResponse(
        Guid StoreId,
        string StoreCd,
        string StoreName,
        Guid AreaId,
        string AreaCd,
        string AreaName,
        Guid StoreTypeId,
        string StoreTypeCd,
        string StoreTypeName,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    [RequirePermission("stores.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? storeCd,
        [FromQuery] string? storeName,
        [FromQuery] Guid? areaId,
        [FromQuery] Guid? storeTypeId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);

        var total = await _storeRepo.CountAsync(storeCd, storeName, areaId, storeTypeId, isActive, ct);
        var items = await _storeRepo.ListAsync(storeCd, storeName, areaId, storeTypeId, isActive, skip, pageSize, ct);

        return Ok(new { total, page, pageSize, items = items.Select(Map) });
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("stores.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _storeRepo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    [RequirePermission("stores.create")]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var maxCode = await _storeRepo.GetMaxStoreCodeAsync(ct);
        var storeCd = CodeGenerator.NextStoreCode(maxCode);

        var entity = new Store(storeCd, req.StoreName.Trim(), req.AreaId, req.StoreTypeId, req.IsActive, userId);

        await _storeRepo.AddAsync(entity, ct);
        await _storeRepo.SaveChangesAsync(ct);

        var created = await _storeRepo.GetByIdAsync(entity.StoreId, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.StoreId }, Map(created!));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("stores.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateStoreRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _storeRepo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.Update(req.StoreName.Trim(), req.AreaId, req.StoreTypeId, userId);
        await _storeRepo.SaveChangesAsync(ct);

        var updated = await _storeRepo.GetByIdAsync(entity.StoreId, ct);
        return Ok(Map(updated!));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("stores.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _storeRepo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.SetActivation(req.IsActive, userId);
        await _storeRepo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    private static StoreResponse Map(Store x)
        => new(
            x.StoreId,
            x.StoreCd,
            x.StoreName,
            x.AreaId,
            x.Area?.AreaCd ?? string.Empty,
            x.Area?.AreaName ?? string.Empty,
            x.StoreTypeId,
            x.StoreType?.StoreTypeCd ?? string.Empty,
            x.StoreType?.StoreTypeName ?? string.Empty,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
