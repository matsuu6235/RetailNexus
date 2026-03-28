namespace RetailNexus.Domain.Entities;

public class Permission
{
    public Guid PermissionId { get; init; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    private Permission() { }

    public Permission(string permissionCode, string permissionName, string category)
    {
        PermissionCode = permissionCode;
        PermissionName = permissionName;
        Category = category;
    }
}
