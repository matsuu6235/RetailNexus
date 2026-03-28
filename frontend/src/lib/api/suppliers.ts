import { apiGet, apiPost, apiPut } from "./client";
import type { Supplier } from "@/types/suppliers";

export type SupplierListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: Supplier[];
};

export type CreateSupplierRequest = {
  supplierName: string;
  phoneNumber?: string;
  email?: string;
};

export type UpdateSupplierRequest = CreateSupplierRequest;

export type SupplierSearchParams = {
  supplierCode?: string;
  supplierName?: string;
  phoneNumber?: string;
  email?: string;
  isActive?: "all" | "active" | "inactive";
};

export async function getSuppliers(
  page = 1,
  pageSize = 20,
  searchParams?: SupplierSearchParams
): Promise<SupplierListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.supplierCode?.trim()) params.set("supplierCode", searchParams.supplierCode.trim());
  if (searchParams?.supplierName?.trim()) params.set("supplierName", searchParams.supplierName.trim());
  if (searchParams?.phoneNumber?.trim()) params.set("phoneNumber", searchParams.phoneNumber.trim());
  if (searchParams?.email?.trim()) params.set("email", searchParams.email.trim());

  if (searchParams?.isActive === "active") params.set("isActive", "true");
  if (searchParams?.isActive === "inactive") params.set("isActive", "false");

  return apiGet<SupplierListResponse>(`/api/suppliers?${params.toString()}`);
}

export async function getSupplierById(id: string): Promise<Supplier> {
  return apiGet<Supplier>(`/api/suppliers/${id}`);
}

export async function createSupplier(body: CreateSupplierRequest): Promise<Supplier> {
  return apiPost<CreateSupplierRequest, Supplier>("/api/suppliers", body);
}

export async function updateSupplier(id: string, body: UpdateSupplierRequest): Promise<Supplier> {
  return apiPut<UpdateSupplierRequest, Supplier>(`/api/suppliers/${id}`, body);
}

export async function changeSupplierActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/suppliers/${id}/activation`, { isActive });
}