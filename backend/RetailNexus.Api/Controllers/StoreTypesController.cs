using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StoreTypesController : ControllerBase
{
    private readonly IStoreTypeRepository _repo;
    private readonly IValidator<CreateStoreTypeRequest> _createValidator;
    private readonly IValidator<UpdateStoreTypeRequest> _updateValidator;
    private readonly IValidator<ReorderStoreTypesRequest> _reorderValidator;

    public StoreTypesController(
        IStoreTypeRepository repo,
        IValidator<CreateStoreTypeRequest> createValidator,
        IValidator<UpdateStoreTypeRequest> updateValidator,
        IValidator<ReorderStoreTypesRequest> reorderValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateStoreTypeRequest(string StoreTypeCd, string StoreTypeName, bool IsActive = true);
    public sealed record UpdateStoreTypeRequest(string StoreTypeCd, string StoreTypeName, bool IsActive = true);
    public sealed record ReorderStoreTypesRequest(IReadOnlyList<Guid> StoreTypeIds);

    public sealed record StoreTypeResponse(
        Guid StoreTypeId,
        string StoreTypeCd,
        string StoreTypeName,
        int DisplayOrder,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? storeTypeCd,
        [FromQuery] string? storeTypeName,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var items = await _repo.ListAsync(storeTypeCd, storeTypeName, isActive, ct);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreTypeRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var code = req.StoreTypeCd.Trim();
        var name = req.StoreTypeName.Trim();
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);
        var entity = new StoreType(code, name, nextDisplayOrder, req.IsActive, userId);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.StoreTypeId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreTypeRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateStoreTypeRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.Update(req.StoreTypeCd.Trim(), req.StoreTypeName.Trim(), req.IsActive, userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStoreTypesRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _reorderValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var distinctIds = req.StoreTypeIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

        var orderMap = req.StoreTypeIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.StoreTypeId], userId);
        }

        await _repo.SaveChangesAsync(ct);

        return Ok(entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.StoreTypeCd)
            .Select(Map));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
    }

    private static StoreTypeResponse Map(StoreType x)
        => new(
            x.StoreTypeId,
            x.StoreTypeCd,
            x.StoreTypeName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
