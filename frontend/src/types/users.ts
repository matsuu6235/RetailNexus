export type UserRole = {
  roleId: string;
  roleName: string;
};

export type User = {
  userId: string;
  loginId: string;
  userName: string;
  email: string | null;
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string;
  roles: UserRole[];
};
