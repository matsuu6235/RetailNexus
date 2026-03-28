import { apiGet, apiPost, apiPut } from "./client";
import type { Role, Permission } from "@/types/roles";

export type CreateRoleRequest = {
  roleName: string;
  description: string | null;
  permissionIds: string[];
};

export type UpdateRoleRequest = CreateRoleRequest;

export async function getRoles(): Promise<Role[]> {
  return apiGet<Role[]>("/api/roles");
}

export async function getRoleById(id: string): Promise<Role> {
  return apiGet<Role>(`/api/roles/${id}`);
}

export async function createRole(body: CreateRoleRequest): Promise<Role> {
  return apiPost<CreateRoleRequest, Role>("/api/roles", body);
}

export async function updateRole(id: string, body: UpdateRoleRequest): Promise<Role> {
  return apiPut<UpdateRoleRequest, Role>(`/api/roles/${id}`, body);
}

export async function getPermissions(): Promise<Permission[]> {
  return apiGet<Permission[]>("/api/roles/permissions");
}

export async function changeRoleActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/roles/${id}/activation`, { isActive });
}
