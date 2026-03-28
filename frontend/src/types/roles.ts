export type RolePermission = {
  permissionId: string;
  permissionCode: string;
};

export type Role = {
  roleId: string;
  roleName: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  permissions: RolePermission[];
};

export type Permission = {
  permissionId: string;
  permissionCode: string;
  permissionName: string;
  category: string;
};
