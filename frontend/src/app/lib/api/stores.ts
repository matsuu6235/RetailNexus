import { apiGet, apiPost, apiPut } from "./client";
import type { Store } from "../../types/stores";

export type StoreListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: Store[];
};

export type CreateStoreRequest = {
  storeCd: string;
  storeName: string;
  areaId: string;
  storeTypeId: string;
  isActive: boolean;
};

export type UpdateStoreRequest = CreateStoreRequest;

export type StoreSearchParams = {
  storeCd?: string;
  storeName?: string;
  areaId?: string;
  storeTypeId?: string;
  isActive?: "all" | "active" | "inactive";
};

export async function getStores(
  page = 1,
  pageSize = 20,
  searchParams?: StoreSearchParams
): Promise<StoreListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.storeCd?.trim()) params.set("storeCd", searchParams.storeCd.trim());
  if (searchParams?.storeName?.trim()) params.set("storeName", searchParams.storeName.trim());
  if (searchParams?.areaId?.trim()) params.set("areaId", searchParams.areaId);
  if (searchParams?.storeTypeId?.trim()) params.set("storeTypeId", searchParams.storeTypeId);
  if (searchParams?.isActive === "active") params.set("isActive", "true");
  if (searchParams?.isActive === "inactive") params.set("isActive", "false");

  return apiGet<StoreListResponse>(`/api/stores?${params.toString()}`);
}

export async function getStoreById(id: string): Promise<Store> {
  return apiGet<Store>(`/api/stores/${id}`);
}

export async function createStore(body: CreateStoreRequest): Promise<Store> {
  return apiPost<CreateStoreRequest, Store>("/api/stores", body);
}

export async function updateStore(id: string, body: UpdateStoreRequest): Promise<Store> {
  return apiPut<UpdateStoreRequest, Store>(`/api/stores/${id}`, body);
}