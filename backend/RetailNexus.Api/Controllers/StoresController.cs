using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StoresController : ControllerBase
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

    public sealed record CreateStoreRequest(string StoreCd, string StoreName, Guid AreaId, Guid StoreTypeId, bool IsActive = true);
    public sealed record UpdateStoreRequest(string StoreCd, string StoreName, Guid AreaId, Guid StoreTypeId, bool IsActive = true);
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
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var skip = (page - 1) * pageSize;

        var total = await _storeRepo.CountAsync(storeCd, storeName, areaId, storeTypeId, isActive, ct);
        var items = await _storeRepo.ListAsync(storeCd, storeName, areaId, storeTypeId, isActive, skip, pageSize, ct);

        return Ok(new { total, page, pageSize, items = items.Select(Map) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _storeRepo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = new Store(req.StoreCd.Trim(), req.StoreName.Trim(), req.AreaId, req.StoreTypeId, req.IsActive, userId);

        await _storeRepo.AddAsync(entity, ct);
        await _storeRepo.SaveChangesAsync(ct);

        var created = await _storeRepo.GetByIdAsync(entity.StoreId, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.StoreId }, Map(created!));
    }

    [HttpPut("{id:guid}")]
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

        entity.Update(req.StoreCd.Trim(), req.StoreName.Trim(), req.AreaId, req.StoreTypeId, req.IsActive, userId);
        await _storeRepo.SaveChangesAsync(ct);

        var updated = await _storeRepo.GetByIdAsync(entity.StoreId, ct);
        return Ok(Map(updated!));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
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
