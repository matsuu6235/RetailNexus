using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    public StoreTypesController(IStoreTypeRepository repo)
    {
        _repo = repo;
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

        var code = req.StoreTypeCd.Trim();
        var name = req.StoreTypeName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("StoreTypeCd and StoreTypeName are required.");
        
        if (code.Length > 2)
            return BadRequest("StoreTypeCd must be 2 characters or less.");

        var duplicate = await _repo.GetByCodeAsync(code, ct);
        if (duplicate is not null)
            return Conflict("StoreTypeCd already exists.");

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

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        var code = req.StoreTypeCd.Trim();
        var name = req.StoreTypeName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("StoreTypeCd and StoreTypeName are required.");
        
        if (code.Length > 2)
            return BadRequest("StoreTypeCd must be 2 characters or less.");

        var duplicate = await _repo.GetByCodeAsync(code, ct);
        if (duplicate is not null && duplicate.StoreTypeId != id)
            return Conflict("StoreTypeCd already exists.");

        entity.Update(code, name, req.IsActive, userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStoreTypesRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        if (req.StoreTypeIds is null || req.StoreTypeIds.Count == 0)
            return BadRequest("StoreTypeIds is required.");

        var distinctIds = req.StoreTypeIds.Distinct().ToArray();
        if (distinctIds.Length != req.StoreTypeIds.Count)
            return BadRequest("StoreTypeIds contains duplicates.");

        var entities = await _repo.GetByIdsAsync(distinctIds, ct);
        if (entities.Count != distinctIds.Length)
            return BadRequest("Some store types were not found.");

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