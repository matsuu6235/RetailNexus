using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : BaseController
{
    private readonly IUserRepository _userRepo;
    private readonly RetailNexusDbContext _db;

    public UsersController(IUserRepository userRepo, RetailNexusDbContext db)
    {
        _userRepo = userRepo;
        _db = db;
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

        if (string.IsNullOrWhiteSpace(req.LoginId))
            return BadRequest(new { LoginId = new[] { "ログインIDは必須です。" } });
        if (string.IsNullOrWhiteSpace(req.UserName))
            return BadRequest(new { UserName = new[] { "ユーザー名は必須です。" } });
        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { Password = new[] { "パスワードは必須です。" } });
        if (req.Password.Length < 8)
            return BadRequest(new { Password = new[] { "パスワードは8文字以上で入力してください。" } });

        var existing = await _userRepo.FindByLoginIdAsync(req.LoginId.Trim(), ct);
        if (existing is not null)
            return BadRequest(new { LoginId = new[] { "このログインIDは既に使用されています。" } });

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = new User(req.LoginId.Trim(), req.UserName.Trim(), req.Email, hash, req.IsActive, actorId, actorId);
        await _userRepo.AddAsync(user, ct);
        await _userRepo.SaveChangesAsync(ct);

        if (req.RoleIds.Count > 0)
        {
            foreach (var roleId in req.RoleIds.Distinct())
            {
                _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = roleId });
            }
            await _db.SaveChangesAsync(ct);
        }

        var created = await _userRepo.FindByIdWithRolesAsync(user.UserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, MapUser(created!));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("users.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.LoginId))
            return BadRequest(new { LoginId = new[] { "ログインIDは必須です。" } });
        if (string.IsNullOrWhiteSpace(req.UserName))
            return BadRequest(new { UserName = new[] { "ユーザー名は必須です。" } });

        var user = await _userRepo.FindByIdWithRolesAsync(id, ct);
        if (user is null)
            return NotFound();

        var duplicate = await _userRepo.FindByLoginIdAsync(req.LoginId.Trim(), ct);
        if (duplicate is not null && duplicate.UserId != id)
            return BadRequest(new { LoginId = new[] { "このログインIDは既に使用されています。" } });

        user.UpdateProfile(req.LoginId.Trim(), req.UserName.Trim(), req.Email, actorId);

        // ロールの更新
        var existingRoles = await _db.UserRoles.Where(ur => ur.UserId == id).ToListAsync(ct);
        _db.UserRoles.RemoveRange(existingRoles);
        foreach (var roleId in req.RoleIds.Distinct())
        {
            _db.UserRoles.Add(new UserRole { UserId = id, RoleId = roleId });
        }

        await _db.SaveChangesAsync(ct);

        var updated = await _userRepo.FindByIdWithRolesAsync(id, ct);
        return Ok(MapUser(updated!));
    }

    [HttpPut("{id:guid}/password")]
    [RequirePermission("users.edit")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest(new { NewPassword = new[] { "パスワードは必須です。" } });
        if (req.NewPassword.Length < 8)
            return BadRequest(new { NewPassword = new[] { "パスワードは8文字以上で入力してください。" } });

        var user = await _userRepo.FindByIdAsync(id, ct);
        if (user is null)
            return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userRepo.SaveChangesAsync(ct);

        return NoContent();
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("users.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorId))
            return Unauthorized();

        var user = await _userRepo.FindByIdAsync(id, ct);
        if (user is null)
            return NotFound();

        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = actorId;
        await _userRepo.SaveChangesAsync(ct);

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
