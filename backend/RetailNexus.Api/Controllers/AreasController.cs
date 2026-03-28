using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AreasController : ControllerBase
{
    private readonly IAreaRepository _repo;
    private readonly IValidator<CreateAreaRequest> _createValidator;
    private readonly IValidator<UpdateAreaRequest> _updateValidator;
    private readonly IValidator<ReorderAreasRequest> _reorderValidator;

    public AreasController(
        IAreaRepository repo,
        IValidator<CreateAreaRequest> createValidator,
        IValidator<UpdateAreaRequest> updateValidator,
        IValidator<ReorderAreasRequest> reorderValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateAreaRequest(string AreaCd, string AreaName, bool IsActive = true);
    public sealed record UpdateAreaRequest(string AreaCd, string AreaName);
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
    [RequirePermission("areas.view")]
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
    [RequirePermission("areas.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    [RequirePermission("areas.create")]
    public async Task<IActionResult> Create([FromBody] CreateAreaRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var code = req.AreaCd.Trim();
        var name = req.AreaName.Trim();
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);
        var entity = new Area(code, name, nextDisplayOrder, req.IsActive, userId);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.AreaId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("areas.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAreaRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateAreaRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.Update(req.AreaCd.Trim(), req.AreaName.Trim(), userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("areas.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.SetActivation(req.IsActive, userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    [RequirePermission("areas.edit")]
    public async Task<IActionResult> Reorder([FromBody] ReorderAreasRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _reorderValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var distinctIds = req.AreaIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

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
