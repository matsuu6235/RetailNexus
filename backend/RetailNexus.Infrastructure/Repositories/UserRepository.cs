using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly RetailNexusDbContext _db;

    public UserRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByLoginIdAsync(string loginId, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.LoginId == loginId, ct);

    public Task<User?> FindByLoginIdWithRolesAsync(string loginId, CancellationToken ct)
        => _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.LoginId == loginId, ct);

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public Task<User?> FindByIdWithRolesAsync(Guid userId, CancellationToken ct)
        => _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct)
        => _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.LoginId)
            .ToListAsync(ct);

    public async Task<List<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync(ct);

    public async Task<List<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct)
        => await _db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
