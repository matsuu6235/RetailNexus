import { apiGet, apiPost, apiPut } from "./client";
import type {
  PurchaseOrder,
  PurchaseOrderListItem,
  PurchaseOrderMessage,
  PurchaseOrderStatus,
} from "@/types/purchaseOrders";

export type PurchaseOrderListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: PurchaseOrderListItem[];
};

export type CreateDetailRequest = {
  productId: string;
  quantity: number;
  unitPrice: number;
};

export type CreatePurchaseOrderRequest = {
  supplierId: string;
  storeId: string;
  orderDate: string;
  desiredDeliveryDate?: string | null;
  note?: string;
  details: CreateDetailRequest[];
};

export type UpdateDetailRequest = {
  purchaseOrderDetailId?: string | null;
  productId: string;
  quantity: number;
  unitPrice: number;
};

export type UpdatePurchaseOrderRequest = {
  supplierId: string;
  storeId: string;
  orderDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  note?: string;
  details: UpdateDetailRequest[];
};

export type PurchaseOrderSearchParams = {
  orderNumber?: string;
  supplierId?: string;
  storeId?: string;
  status?: string;
  orderDateFrom?: string;
  orderDateTo?: string;
  isActive?: "all" | "active" | "inactive";
};

export async function getPurchaseOrders(
  page = 1,
  pageSize = 20,
  searchParams?: PurchaseOrderSearchParams
): Promise<PurchaseOrderListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.orderNumber?.trim()) params.set("orderNumber", searchParams.orderNumber.trim());
  if (searchParams?.supplierId) params.set("supplierId", searchParams.supplierId);
  if (searchParams?.storeId) params.set("storeId", searchParams.storeId);
  if (searchParams?.status) params.set("status", searchParams.status);
  if (searchParams?.orderDateFrom) params.set("orderDateFrom", searchParams.orderDateFrom);
  if (searchParams?.orderDateTo) params.set("orderDateTo", searchParams.orderDateTo);

  if (searchParams?.isActive === "active") params.set("isActive", "true");
  if (searchParams?.isActive === "inactive") params.set("isActive", "false");

  return apiGet<PurchaseOrderListResponse>(`/api/purchase-orders?${params.toString()}`);
}

export async function getPurchaseOrderById(id: string): Promise<PurchaseOrder> {
  return apiGet<PurchaseOrder>(`/api/purchase-orders/${id}`);
}

export async function getPurchaseOrderByNumber(orderNumber: string): Promise<PurchaseOrder> {
  return apiGet<PurchaseOrder>(`/api/purchase-orders/by-number/${orderNumber}`);
}

export async function createPurchaseOrder(body: CreatePurchaseOrderRequest): Promise<PurchaseOrder> {
  return apiPost<CreatePurchaseOrderRequest, PurchaseOrder>("/api/purchase-orders", body);
}

export async function updatePurchaseOrder(id: string, body: UpdatePurchaseOrderRequest): Promise<PurchaseOrder> {
  return apiPut<UpdatePurchaseOrderRequest, PurchaseOrder>(`/api/purchase-orders/${id}`, body);
}

export async function submitForApproval(id: string): Promise<PurchaseOrder> {
  return apiPut<Record<string, never>, PurchaseOrder>(`/api/purchase-orders/${id}/submit`, {});
}

export async function approvePurchaseOrder(id: string): Promise<PurchaseOrder> {
  return apiPut<Record<string, never>, PurchaseOrder>(`/api/purchase-orders/${id}/approve`, {});
}

export async function rejectPurchaseOrder(id: string): Promise<PurchaseOrder> {
  return apiPut<Record<string, never>, PurchaseOrder>(`/api/purchase-orders/${id}/reject`, {});
}

export async function changePurchaseOrderStatus(id: string, status: PurchaseOrderStatus): Promise<PurchaseOrder> {
  return apiPut<{ status: PurchaseOrderStatus }, PurchaseOrder>(`/api/purchase-orders/${id}/status`, { status });
}

export async function changePurchaseOrderActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/purchase-orders/${id}/activation`, { isActive });
}

// ── メッセージ ──

export async function getPurchaseOrderMessages(purchaseOrderId: string): Promise<PurchaseOrderMessage[]> {
  return apiGet<PurchaseOrderMessage[]>(`/api/purchase-orders/${purchaseOrderId}/messages`);
}

export async function sendPurchaseOrderMessage(purchaseOrderId: string, body: string): Promise<PurchaseOrderMessage> {
  return apiPost<{ body: string }, PurchaseOrderMessage>(`/api/purchase-orders/${purchaseOrderId}/messages`, { body });
}
