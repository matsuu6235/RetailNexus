using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IRoleService
{
    Task<Role> CreateAsync(string roleName, string? description, bool isActive, List<Guid> permissionIds, Guid actorId, CancellationToken ct);
    Task<Role> UpdateAsync(Guid id, string roleName, string? description, List<Guid> permissionIds, Guid actorId, CancellationToken ct);
    Task ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
