using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByLoginIdAsync(string loginId, CancellationToken ct);
    Task<User?> FindByLoginIdWithRolesAsync(string loginId, CancellationToken ct);
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct);
    Task<User?> FindByIdWithRolesAsync(Guid userId, CancellationToken ct);
    Task<List<User>> GetAllAsync(CancellationToken ct);
    Task<List<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct);
    Task<List<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task ReplaceUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}