using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    public async Task<Role> CreateAsync(string roleName, string? description, bool isActive, List<Guid> permissionIds, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new BusinessRuleException("RoleName", "ロール名は必須です。");

        var existing = await _roleRepo.FindByNameAsync(roleName.Trim(), ct);
        if (existing is not null)
            throw new DuplicateException("RoleName", "このロール名は既に使用されています。");

        var role = new Role(roleName.Trim(), description?.Trim());
        role.IsActive = isActive;
        await _roleRepo.AddAsync(role, ct);
        await _roleRepo.SaveChangesAsync(ct);

        if (permissionIds.Count > 0)
        {
            await _roleRepo.ReplaceRolePermissionsAsync(role.RoleId, permissionIds, ct);
            await _roleRepo.SaveChangesAsync(ct);
        }

        var created = await _roleRepo.FindByIdWithPermissionsAsync(role.RoleId, ct);
        return created!;
    }

    public async Task<Role> UpdateAsync(Guid id, string roleName, string? description, List<Guid> permissionIds, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new BusinessRuleException("RoleName", "ロール名は必須です。");

        var role = await _roleRepo.FindByIdWithPermissionsAsync(id, ct)
            ?? throw new EntityNotFoundException("Role", id);

        var duplicate = await _roleRepo.FindByNameAsync(roleName.Trim(), ct);
        if (duplicate is not null && duplicate.RoleId != id)
            throw new DuplicateException("RoleName", "このロール名は既に使用されています。");

        role.RoleName = roleName.Trim();
        role.Description = description?.Trim();
        role.UpdatedAt = DateTimeOffset.UtcNow;

        await _roleRepo.ReplaceRolePermissionsAsync(id, permissionIds, ct);
        await _roleRepo.SaveChangesAsync(ct);

        var updated = await _roleRepo.FindByIdWithPermissionsAsync(id, ct);
        return updated!;
    }

    public async Task ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var role = await _roleRepo.FindByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Role", id);

        role.IsActive = isActive;
        role.UpdatedAt = DateTimeOffset.UtcNow;
        await _roleRepo.SaveChangesAsync(ct);
    }
}
