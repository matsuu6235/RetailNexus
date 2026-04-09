using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Api.Contracts;
using RetailNexus.Api.Controllers;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

[Route("api/[controller]")]
[Authorize]
public sealed class StoreTypesController : BaseController
{
    private readonly IStoreTypeRepository _repo;
    private readonly IStoreTypeService _service;
    private readonly IValidator<CreateStoreTypeRequest> _createValidator;
    private readonly IValidator<UpdateStoreTypeRequest> _updateValidator;
    private readonly IValidator<ReorderStoreTypesRequest> _reorderValidator;

    public StoreTypesController(
        IStoreTypeRepository repo,
        IStoreTypeService service,
        IValidator<CreateStoreTypeRequest> createValidator,
        IValidator<UpdateStoreTypeRequest> updateValidator,
        IValidator<ReorderStoreTypesRequest> reorderValidator)
    {
        _repo = repo;
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateStoreTypeRequest(string StoreTypeCode, string StoreTypeName, bool IsActive = true) : IStoreTypeRequest;
    public sealed record UpdateStoreTypeRequest(string StoreTypeCode, string StoreTypeName) : IStoreTypeRequest;
    public sealed record ReorderStoreTypesRequest(IReadOnlyList<Guid> StoreTypeIds);

    public sealed record StoreTypeResponse(
        Guid StoreTypeId,
        string StoreTypeCode,
        string StoreTypeName,
        int DisplayOrder,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    [RequirePermission("store-types.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? storeTypeCode,
        [FromQuery] string? storeTypeName,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var items = await _repo.ListAsync(storeTypeCode, storeTypeName, isActive, ct);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("store-types.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    [RequirePermission("store-types.create")]
    public async Task<IActionResult> Create([FromBody] CreateStoreTypeRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _service.CreateAsync(req.StoreTypeCode, req.StoreTypeName, req.IsActive, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.StoreTypeId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("store-types.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreTypeRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateStoreTypeRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _service.UpdateAsync(id, req.StoreTypeCode, req.StoreTypeName, userId, ct);
        return Ok(Map(entity));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("store-types.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _service.ChangeActivationAsync(id, req.IsActive, userId, ct);
        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    [RequirePermission("store-types.edit")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStoreTypesRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _reorderValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entities = await _service.ReorderAsync(req.StoreTypeIds, userId, ct);
        return Ok(entities.Select(Map));
    }

    private static StoreTypeResponse Map(StoreType x)
        => new(
            x.StoreTypeId,
            x.StoreTypeCode,
            x.StoreTypeName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
