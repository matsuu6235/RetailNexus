export type PurchaseOrderStatus =
  | 0  // Draft
  | 1  // AwaitingApproval
  | 2  // Approved
  | 3  // SupplierConfirmed
  | 4  // Preparing
  | 5  // Shipped
  | 6  // Received
  | 91 // CancelRequested
  | 92 // Cancelled
  | 93; // SupplierCancelled

export const purchaseOrderStatusLabels: Record<PurchaseOrderStatus, string> = {
  0: "下書き",
  1: "承認待ち",
  2: "承認済",
  3: "仕入先確認済",
  4: "出荷準備中",
  5: "出荷済",
  6: "入荷済",
  91: "キャンセル依頼済",
  92: "キャンセル済",
  93: "仕入先キャンセル",
};

export type PurchaseOrderListItem = {
  purchaseOrderId: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  storeId: string;
  storeName: string;
  orderDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  receivedDate?: string | null;
  status: PurchaseOrderStatus;
  totalAmount: number;
  approvedBy?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type PurchaseOrderDetail = {
  purchaseOrderDetailId: string;
  productId: string;
  productCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
};

export type PurchaseOrder = {
  purchaseOrderId: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  storeId: string;
  storeName: string;
  orderDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  receivedDate?: string | null;
  status: PurchaseOrderStatus;
  totalAmount: number;
  note?: string | null;
  approvedBy?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
  details: PurchaseOrderDetail[];
};
