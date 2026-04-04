using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(CancellationToken ct);
    Task<Role?> FindByIdAsync(Guid roleId, CancellationToken ct);
    Task<Role?> FindByIdWithPermissionsAsync(Guid roleId, CancellationToken ct);
    Task<Role?> FindByNameAsync(string roleName, CancellationToken ct);
    Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct);
    Task AddAsync(Role role, CancellationToken ct);
    Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
