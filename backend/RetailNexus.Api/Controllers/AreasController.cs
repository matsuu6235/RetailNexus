using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Api.Contracts;
using RetailNexus.Api.Controllers;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class AreasController : BaseController
{
    private readonly IAreaRepository _repo;
    private readonly IAreaService _service;
    private readonly IValidator<CreateAreaRequest> _createValidator;
    private readonly IValidator<UpdateAreaRequest> _updateValidator;
    private readonly IValidator<ReorderAreasRequest> _reorderValidator;

    public AreasController(
        IAreaRepository repo,
        IAreaService service,
        IValidator<CreateAreaRequest> createValidator,
        IValidator<UpdateAreaRequest> updateValidator,
        IValidator<ReorderAreasRequest> reorderValidator)
    {
        _repo = repo;
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateAreaRequest(string AreaCode, string AreaName, bool IsActive = true) : IAreaRequest;
    public sealed record UpdateAreaRequest(string AreaCode, string AreaName) : IAreaRequest;
    public sealed record ReorderAreasRequest(IReadOnlyList<Guid> AreaIds);
    public sealed record AreaResponse(
        Guid AreaId,
        string AreaCode,
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
        [FromQuery] string? areaCode,
        [FromQuery] string? areaName,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);

        var total = await _repo.CountAsync(areaCode, areaName, isActive, ct);
        var items = await _repo.ListAsync(areaCode, areaName, isActive, skip, pageSize, ct);

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

        var entity = await _service.CreateAsync(req.AreaCode, req.AreaName, req.IsActive, userId, ct);
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

        var entity = await _service.UpdateAsync(id, req.AreaCode, req.AreaName, userId, ct);
        return Ok(Map(entity));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("areas.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _service.ChangeActivationAsync(id, req.IsActive, userId, ct);
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

        var entities = await _service.ReorderAsync(req.AreaIds, userId, ct);
        return Ok(entities.Select(Map));
    }

    private static AreaResponse Map(Area x)
        => new(
            x.AreaId,
            x.AreaCode,
            x.AreaName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
