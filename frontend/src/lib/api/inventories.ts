import { apiGet, apiPost } from "./client";
import type { InventoryListItem, InventoryTransactionListItem, InventoryTransactionType } from "@/types/inventories";

export type InventoryListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: InventoryListItem[];
};

export type InventorySearchParams = {
  areaId?: string;
  storeId?: string;
  productCategoryId?: string;
  productCode?: string;
  stockStatus?: string;
};

export async function getInventories(
  page = 1,
  pageSize = 20,
  searchParams?: InventorySearchParams
): Promise<InventoryListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.areaId) params.set("areaId", searchParams.areaId);
  if (searchParams?.storeId) params.set("storeId", searchParams.storeId);
  if (searchParams?.productCategoryId) params.set("productCategoryId", searchParams.productCategoryId);
  if (searchParams?.productCode?.trim()) params.set("productCode", searchParams.productCode.trim());
  if (searchParams?.stockStatus && searchParams.stockStatus !== "all") params.set("stockStatus", searchParams.stockStatus);

  return apiGet<InventoryListResponse>(`/api/inventories?${params.toString()}`);
}

// ── トランザクション ──

export type InventoryTransactionListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: InventoryTransactionListItem[];
};

export type InventoryTransactionSearchParams = {
  storeId?: string;
  productId?: string;
  transactionType?: string;
  dateFrom?: string;
  dateTo?: string;
};

export async function getInventoryTransactions(
  page = 1,
  pageSize = 20,
  searchParams?: InventoryTransactionSearchParams
): Promise<InventoryTransactionListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.storeId) params.set("storeId", searchParams.storeId);
  if (searchParams?.productId) params.set("productId", searchParams.productId);
  if (searchParams?.transactionType) params.set("transactionType", searchParams.transactionType);
  if (searchParams?.dateFrom) params.set("dateFrom", searchParams.dateFrom);
  if (searchParams?.dateTo) params.set("dateTo", searchParams.dateTo);

  return apiGet<InventoryTransactionListResponse>(`/api/inventory-transactions?${params.toString()}`);
}

export type ManualTransactionRequest = {
  storeId: string;
  productId: string;
  transactionType: InventoryTransactionType;
  quantityChange: number;
  note?: string;
};

export async function createManualTransaction(body: ManualTransactionRequest): Promise<InventoryTransactionListItem> {
  return apiPost<ManualTransactionRequest, InventoryTransactionListItem>("/api/inventory-transactions/manual", body);
}
