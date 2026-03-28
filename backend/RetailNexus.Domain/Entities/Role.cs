namespace RetailNexus.Domain.Entities;

public class Role
{
    public Guid RoleId { get; init; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? UpdatedBy { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    private Role() { }

    public void SetActivation(bool isActive, Guid actorId)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
    }

    public Role(string roleName, string? description, Guid? createdBy = null)
    {
        RoleName = roleName.Trim();
        Description = description?.Trim();
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}
