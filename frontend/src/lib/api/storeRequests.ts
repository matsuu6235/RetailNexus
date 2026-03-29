import { apiGet, apiPost, apiPut } from "./client";
import type {
  StoreRequest,
  StoreRequestListItem,
  StoreRequestStatus,
} from "@/types/storeRequests";

export type StoreRequestListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: StoreRequestListItem[];
};

export type CreateDetailRequest = {
  productId: string;
  quantity: number;
};

export type CreateStoreRequestRequest = {
  fromStoreId: string;
  toStoreId: string;
  requestDate: string;
  desiredDeliveryDate?: string | null;
  note?: string;
  details: CreateDetailRequest[];
};

export type UpdateDetailRequest = {
  storeRequestDetailId?: string | null;
  productId: string;
  quantity: number;
};

export type UpdateStoreRequestRequest = {
  fromStoreId: string;
  toStoreId: string;
  requestDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  note?: string;
  details: UpdateDetailRequest[];
};

export type StoreRequestSearchParams = {
  requestNumber?: string;
  fromStoreId?: string;
  toStoreId?: string;
  status?: string;
  requestDateFrom?: string;
  requestDateTo?: string;
  isActive?: "all" | "active" | "inactive";
};

export async function getStoreRequests(
  page = 1,
  pageSize = 20,
  searchParams?: StoreRequestSearchParams
): Promise<StoreRequestListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.requestNumber?.trim()) params.set("requestNumber", searchParams.requestNumber.trim());
  if (searchParams?.fromStoreId) params.set("fromStoreId", searchParams.fromStoreId);
  if (searchParams?.toStoreId) params.set("toStoreId", searchParams.toStoreId);
  if (searchParams?.status) params.set("status", searchParams.status);
  if (searchParams?.requestDateFrom) params.set("requestDateFrom", searchParams.requestDateFrom);
  if (searchParams?.requestDateTo) params.set("requestDateTo", searchParams.requestDateTo);

  if (searchParams?.isActive === "active") params.set("isActive", "true");
  if (searchParams?.isActive === "inactive") params.set("isActive", "false");

  return apiGet<StoreRequestListResponse>(`/api/store-requests?${params.toString()}`);
}

export async function getStoreRequestById(id: string): Promise<StoreRequest> {
  return apiGet<StoreRequest>(`/api/store-requests/${id}`);
}

export async function createStoreRequest(body: CreateStoreRequestRequest): Promise<StoreRequest> {
  return apiPost<CreateStoreRequestRequest, StoreRequest>("/api/store-requests", body);
}

export async function updateStoreRequest(id: string, body: UpdateStoreRequestRequest): Promise<StoreRequest> {
  return apiPut<UpdateStoreRequestRequest, StoreRequest>(`/api/store-requests/${id}`, body);
}

export async function submitStoreRequestForApproval(id: string): Promise<StoreRequest> {
  return apiPut<Record<string, never>, StoreRequest>(`/api/store-requests/${id}/submit`, {});
}

export async function approveStoreRequest(id: string): Promise<StoreRequest> {
  return apiPut<Record<string, never>, StoreRequest>(`/api/store-requests/${id}/approve`, {});
}

export async function rejectStoreRequest(id: string): Promise<StoreRequest> {
  return apiPut<Record<string, never>, StoreRequest>(`/api/store-requests/${id}/reject`, {});
}

export async function changeStoreRequestStatus(id: string, status: StoreRequestStatus): Promise<StoreRequest> {
  return apiPut<{ status: StoreRequestStatus }, StoreRequest>(`/api/store-requests/${id}/status`, { status });
}

export async function changeStoreRequestActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/store-requests/${id}/activation`, { isActive });
}
