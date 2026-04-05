using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : BaseController
{
    private readonly IUserRepository _userRepo;
    private readonly IUserService _service;

    public UsersController(IUserRepository userRepo, IUserService service)
    {
        _userRepo = userRepo;
        _service = service;
    }

    public sealed record CreateUserRequest(string LoginId, string UserName, string? Email, string Password, bool IsActive, List<Guid> RoleIds);
    public sealed record UpdateUserRequest(string LoginId, string UserName, string? Email, List<Guid> RoleIds);
    public sealed record ResetPasswordRequest(string NewPassword);
    public sealed record UserResponse(
        Guid UserId,
        string LoginId,
        string UserName,
        string? Email,
        bool IsActive,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        List<UserRoleResponse> Roles);
    public sealed record UserRoleResponse(Guid RoleId, string RoleName);

    [HttpGet]
    [RequirePermission("users.view")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var users = await _userRepo.GetAllAsync(ct);
        return Ok(users.Select(MapUser));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("users.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepo.FindByIdWithRolesAsync(id, ct);
        return user is null ? NotFound() : Ok(MapUser(user));
    }

    [HttpPost]
    [RequirePermission("users.create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorId))
            return Unauthorized();

        var user = await _service.CreateAsync(req.LoginId, req.UserName, req.Email, req.Password, req.IsActive, req.RoleIds, actorId, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, MapUser(user));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("users.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorId))
            return Unauthorized();

        var user = await _service.UpdateAsync(id, req.LoginId, req.UserName, req.Email, req.RoleIds, actorId, ct);
        return Ok(MapUser(user));
    }

    [HttpPut("{id:guid}/password")]
    [RequirePermission("users.edit")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        TryGetCurrentUserId(out var actorId);

        await _service.ResetPasswordAsync(id, req.NewPassword, actorId, ct);
        return NoContent();
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("users.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorId))
            return Unauthorized();

        await _service.ChangeActivationAsync(id, req.IsActive, actorId, ct);
        return NoContent();
    }

    private static UserResponse MapUser(User u) => new(
        u.UserId,
        u.LoginId,
        u.UserName,
        u.Email,
        u.IsActive,
        u.LastLoginAt,
        u.CreatedAt,
        u.UpdatedAt,
        u.UserRoles.Select(ur => new UserRoleResponse(ur.RoleId, ur.Role?.RoleName ?? "")).ToList());
}
