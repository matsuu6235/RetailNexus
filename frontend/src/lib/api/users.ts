import { apiGet, apiPost, apiPut } from "./client";
import type { User } from "@/types/users";

export type CreateUserRequest = {
  loginId: string;
  userName: string;
  email: string | null;
  password: string;
  roleIds: string[];
};

export type UpdateUserRequest = {
  loginId: string;
  userName: string;
  email: string | null;
  roleIds: string[];
};

export type ResetPasswordRequest = {
  newPassword: string;
};

export async function getUsers(): Promise<User[]> {
  return apiGet<User[]>("/api/users");
}

export async function getUserById(id: string): Promise<User> {
  return apiGet<User>(`/api/users/${id}`);
}

export async function createUser(body: CreateUserRequest): Promise<User> {
  return apiPost<CreateUserRequest, User>("/api/users", body);
}

export async function updateUser(id: string, body: UpdateUserRequest): Promise<User> {
  return apiPut<UpdateUserRequest, User>(`/api/users/${id}`, body);
}

export async function resetPassword(id: string, body: ResetPasswordRequest): Promise<void> {
  return apiPut<ResetPasswordRequest, void>(`/api/users/${id}/password`, body);
}

export async function changeUserActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/users/${id}/activation`, { isActive });
}
