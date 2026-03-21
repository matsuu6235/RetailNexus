using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AreasController : ControllerBase
{
    private readonly IAreaRepository _repo;

    public AreasController(IAreaRepository repo)
    {
        _repo = repo;
    }

    public sealed record CreateAreaRequest(string AreaCd, string AreaName, bool IsActive = true);
    public sealed record UpdateAreaRequest(string AreaCd, string AreaName, bool IsActive = true);
    public sealed record ReorderAreasRequest(IReadOnlyList<Guid> AreaIds);
    public sealed record AreaResponse(
        Guid AreaId,
        string AreaCd,
        string AreaName,
        int DisplayOrder,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? areaCd,
        [FromQuery] string? areaName,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var skip = (page - 1) * pageSize;

        var total = await _repo.CountAsync(areaCd, areaName, isActive, ct);
        var items = await _repo.ListAsync(areaCd, areaName, isActive, skip, pageSize, ct);

        return Ok(new { total, page, pageSize, items = items.Select(Map) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAreaRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var code = req.AreaCd.Trim();
        var name = req.AreaName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("AreaCd and AreaName are required.");

        var duplicate = await _repo.GetByCodeAsync(code, ct);
        if (duplicate is not null)
            return Conflict("AreaCd already exists.");

        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);
        var entity = new Area(code, name, nextDisplayOrder, req.IsActive, userId);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.AreaId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAreaRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        var code = req.AreaCd.Trim();
        var name = req.AreaName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("AreaCd and AreaName are required.");

        var duplicate = await _repo.GetByCodeAsync(code, ct);
        if (duplicate is not null && duplicate.AreaId != id)
            return Conflict("AreaCd already exists.");

        entity.Update(code, name, req.IsActive, userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    public async Task<IActionResult> Reorder([FromBody] ReorderAreasRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        if (req.AreaIds is null || req.AreaIds.Count == 0)
            return BadRequest("AreaIds is required.");

        var distinctIds = req.AreaIds.Distinct().ToArray();
        if (distinctIds.Length != req.AreaIds.Count)
            return BadRequest("AreaIds contains duplicates.");

        var entities = await _repo.GetByIdsAsync(distinctIds, ct);
        if (entities.Count != distinctIds.Length)
            return BadRequest("Some areas were not found.");

        var orderMap = req.AreaIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.AreaId], userId);
        }

        await _repo.SaveChangesAsync(ct);

        return Ok(entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.AreaCd)
            .Select(Map));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
    }

    private static AreaResponse Map(Area x)
        => new(
            x.AreaId,
            x.AreaCd,
            x.AreaName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}