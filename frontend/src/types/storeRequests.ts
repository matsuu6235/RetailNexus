export type StoreRequestStatus =
  | 0  // Draft
  | 1  // AwaitingApproval
  | 2  // Approved
  | 3  // Confirmed
  | 4  // Preparing
  | 5  // Shipped
  | 6  // Received
  | 91 // CancelRequested
  | 92 // Cancelled
  | 93; // Rejected

export const storeRequestStatusLabels: Record<StoreRequestStatus, string> = {
  0: "下書き",
  1: "承認待ち",
  2: "承認済",
  3: "確認済",
  4: "出荷準備中",
  5: "出荷済",
  6: "入荷済",
  91: "キャンセル依頼済",
  92: "キャンセル済",
  93: "却下",
};

export type StoreRequestListItem = {
  storeRequestId: string;
  requestNumber: string;
  fromStoreId: string;
  fromStoreName: string;
  toStoreId: string;
  toStoreName: string;
  requestDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  shippedDate?: string | null;
  receivedDate?: string | null;
  status: StoreRequestStatus;
  approvedBy?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type StoreRequestDetail = {
  storeRequestDetailId: string;
  productId: string;
  productCode: string;
  productName: string;
  quantity: number;
};

export type StoreRequest = {
  storeRequestId: string;
  requestNumber: string;
  fromStoreId: string;
  fromStoreName: string;
  toStoreId: string;
  toStoreName: string;
  requestDate: string;
  desiredDeliveryDate?: string | null;
  expectedDeliveryDate?: string | null;
  shippedDate?: string | null;
  receivedDate?: string | null;
  status: StoreRequestStatus;
  note?: string | null;
  approvedBy?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
  details: StoreRequestDetail[];
};
