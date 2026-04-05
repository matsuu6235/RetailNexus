using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class RolesController : BaseController
{
    private readonly IRoleRepository _roleRepo;
    private readonly IRoleService _service;

    public RolesController(IRoleRepository roleRepo, IRoleService service)
    {
        _roleRepo = roleRepo;
        _service = service;
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
        TryGetCurrentUserId(out var actorId);

        var role = await _service.CreateAsync(req.RoleName, req.Description, req.IsActive, req.PermissionIds, actorId, ct);
        return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, MapRole(role));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("roles.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest req, CancellationToken ct)
    {
        TryGetCurrentUserId(out var actorId);

        var role = await _service.UpdateAsync(id, req.RoleName, req.Description, req.PermissionIds, actorId, ct);
        return Ok(MapRole(role));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("roles.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        TryGetCurrentUserId(out var actorId);

        await _service.ChangeActivationAsync(id, req.IsActive, actorId, ct);
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
