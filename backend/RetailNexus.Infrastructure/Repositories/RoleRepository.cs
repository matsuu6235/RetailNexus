using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly RetailNexusDbContext _db;

    public RoleRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<List<Role>> GetAllAsync(CancellationToken ct)
        => _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.RoleName)
            .ToListAsync(ct);

    public Task<Role?> FindByIdAsync(Guid roleId, CancellationToken ct)
        => _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, ct);

    public Task<Role?> FindByIdWithPermissionsAsync(Guid roleId, CancellationToken ct)
        => _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId, ct);

    public Task<Role?> FindByNameAsync(string roleName, CancellationToken ct)
        => _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName, ct);

    public Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct)
        => _db.Permissions.OrderBy(p => p.Category).ThenBy(p => p.PermissionCode).ToListAsync(ct);

    public async Task AddAsync(Role role, CancellationToken ct)
        => await _db.Roles.AddAsync(role, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
