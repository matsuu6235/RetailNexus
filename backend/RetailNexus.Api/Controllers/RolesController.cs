using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Resources;

namespace RetailNexus.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class RolesController : BaseController
{
    private readonly IRoleRepository _roleRepo;
    private readonly IStringLocalizer<SharedMessages> _localizer;

    public RolesController(IRoleRepository roleRepo, IStringLocalizer<SharedMessages> localizer)
    {
        _roleRepo = roleRepo;
        _localizer = localizer;
    }

    public sealed record CreateRoleRequest(string RoleName, string? Description, bool IsActive, List<Guid> PermissionIds);
    public sealed record UpdateRoleRequest(string RoleName, string? Description, List<Guid> PermissionIds);
    public sealed record RoleResponse(
        Guid RoleId,
        string RoleName,
        string? Description,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        List<RolePermissionResponse> Permissions);
    public sealed record RolePermissionResponse(Guid PermissionId, string PermissionCode);
    public sealed record PermissionResponse(Guid PermissionId, string PermissionCode, string PermissionName, string Category);

    [HttpGet]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var roles = await _roleRepo.GetAllAsync(ct);
        return Ok(roles.Select(MapRole));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var role = await _roleRepo.FindByIdWithPermissionsAsync(id, ct);
        return role is null ? NotFound() : Ok(MapRole(role));
    }

    [HttpGet("permissions")]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var permissions = await _roleRepo.GetAllPermissionsAsync(ct);
        return Ok(permissions.Select(p => new PermissionResponse(p.PermissionId, p.PermissionCode, p.PermissionName, p.Category)));
    }

    [HttpPost]
    [RequirePermission("roles.create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RoleName))
            return BadRequest(new { RoleName = new[] { _localizer["Validation_Required", "ロール名"].Value } });

        var existing = await _roleRepo.FindByNameAsync(req.RoleName.Trim(), ct);
        if (existing is not null)
            return BadRequest(new { RoleName = new[] { _localizer["Validation_Duplicate", "ロール名"].Value } });

        var role = new Role(req.RoleName.Trim(), req.Description?.Trim());
        role.IsActive = req.IsActive;
        await _roleRepo.AddAsync(role, ct);
        await _roleRepo.SaveChangesAsync(ct);

        if (req.PermissionIds.Count > 0)
        {
            await _roleRepo.ReplaceRolePermissionsAsync(role.RoleId, req.PermissionIds, ct);
            await _roleRepo.SaveChangesAsync(ct);
        }

        var created = await _roleRepo.FindByIdWithPermissionsAsync(role.RoleId, ct);
        return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, MapRole(created!));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("roles.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RoleName))
            return BadRequest(new { RoleName = new[] { _localizer["Validation_Required", "ロール名"].Value } });

        var role = await _roleRepo.FindByIdWithPermissionsAsync(id, ct);
        if (role is null)
            return NotFound();

        var duplicate = await _roleRepo.FindByNameAsync(req.RoleName.Trim(), ct);
        if (duplicate is not null && duplicate.RoleId != id)
            return BadRequest(new { RoleName = new[] { _localizer["Validation_Duplicate", "ロール名"].Value } });

        role.RoleName = req.RoleName.Trim();
        role.Description = req.Description?.Trim();
        role.UpdatedAt = DateTimeOffset.UtcNow;

        // 権限の更新
        await _roleRepo.ReplaceRolePermissionsAsync(id, req.PermissionIds, ct);
        await _roleRepo.SaveChangesAsync(ct);

        var updated = await _roleRepo.FindByIdWithPermissionsAsync(id, ct);
        return Ok(MapRole(updated!));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("roles.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        var role = await _roleRepo.FindByIdAsync(id, ct);
        if (role is null)
            return NotFound();

        role.IsActive = req.IsActive;
        role.UpdatedAt = DateTimeOffset.UtcNow;
        await _roleRepo.SaveChangesAsync(ct);

        return NoContent();
    }

    private static RoleResponse MapRole(Role r) => new(
        r.RoleId,
        r.RoleName,
        r.Description,
        r.IsActive,
        r.CreatedAt,
        r.UpdatedAt,
        r.RolePermissions.Select(rp => new RolePermissionResponse(rp.PermissionId, rp.Permission?.PermissionCode ?? "")).ToList());
}
