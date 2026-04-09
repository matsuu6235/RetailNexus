import { apiGet, apiPost, apiPut } from "./client";
import type { StoreType } from "@/types/storeTypes";

export type CreateStoreTypeRequest = {
  storeTypeCode: string;
  storeTypeName: string;
};

export type UpdateStoreTypeRequest = CreateStoreTypeRequest;

export type ReorderStoreTypesRequest = {
  storeTypeIds: string[];
};

export async function getStoreTypes(search?: {
  storeTypeCode?: string;
  storeTypeName?: string;
  isActive?: "all" | "active" | "inactive";
}): Promise<StoreType[]> {
  const params = new URLSearchParams();

  if (search?.storeTypeCode?.trim()) params.set("storeTypeCode", search.storeTypeCode.trim());
  if (search?.storeTypeName?.trim()) params.set("storeTypeName", search.storeTypeName.trim());
  if (search?.isActive === "active") params.set("isActive", "true");
  if (search?.isActive === "inactive") params.set("isActive", "false");

  const suffix = params.toString() ? `?${params.toString()}` : "";
  return apiGet<StoreType[]>(`/api/storetypes${suffix}`);
}

export async function getStoreTypeById(id: string): Promise<StoreType> {
  return apiGet<StoreType>(`/api/storetypes/${id}`);
}

export async function createStoreType(body: CreateStoreTypeRequest): Promise<StoreType> {
  return apiPost<CreateStoreTypeRequest, StoreType>("/api/storetypes", body);
}

export async function updateStoreType(id: string, body: UpdateStoreTypeRequest): Promise<StoreType> {
  return apiPut<UpdateStoreTypeRequest, StoreType>(`/api/storetypes/${id}`, body);
}

export async function reorderStoreTypes(storeTypeIds: string[]): Promise<StoreType[]> {
  return apiPut<ReorderStoreTypesRequest, StoreType[]>("/api/storetypes/display-order", { storeTypeIds });
}

export async function changeStoreTypeActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/storetypes/${id}/activation`, { isActive });
}