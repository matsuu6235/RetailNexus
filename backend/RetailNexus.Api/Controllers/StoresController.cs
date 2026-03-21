﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IAreaRepository _areaRepo;
    private readonly IStoreTypeRepository _storeTypeRepo;

    public StoresController(IStoreRepository storeRepo, IAreaRepository areaRepo, IStoreTypeRepository storeTypeRepo)
    {
        _storeRepo = storeRepo;
        _areaRepo = areaRepo;
        _storeTypeRepo = storeTypeRepo;
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

        var code = req.StoreCd.Trim();
        var name = req.StoreName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("StoreCd and StoreName are required.");

        var duplicate = await _storeRepo.GetByCodeAsync(code, ct);
        if (duplicate is not null)
            return Conflict("StoreCd already exists.");

        var area = await _areaRepo.GetByIdAsync(req.AreaId, ct);
        if (area is null)
            return BadRequest("AreaId not found.");
        if (!area.IsActive)
            return BadRequest("AreaId is inactive.");

        var storeType = await _storeTypeRepo.GetByIdAsync(req.StoreTypeId, ct);
        if (storeType is null)
            return BadRequest("StoreTypeId not found.");
        if (!storeType.IsActive)
            return BadRequest("StoreTypeId is inactive.");

        var entity = new Store(code, name, req.AreaId, req.StoreTypeId, req.IsActive, userId);

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

        var entity = await _storeRepo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        var code = req.StoreCd.Trim();
        var name = req.StoreName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest("StoreCd and StoreName are required.");

        var duplicate = await _storeRepo.GetByCodeAsync(code, ct);
        if (duplicate is not null && duplicate.StoreId != id)
            return Conflict("StoreCd already exists.");

        var area = await _areaRepo.GetByIdAsync(req.AreaId, ct);
        if (area is null)
            return BadRequest("AreaId not found.");

        var storeType = await _storeTypeRepo.GetByIdAsync(req.StoreTypeId, ct);
        if (storeType is null)
            return BadRequest("StoreTypeId not found.");

        entity.Update(code, name, req.AreaId, req.StoreTypeId, req.IsActive, userId);
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