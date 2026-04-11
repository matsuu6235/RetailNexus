export type InventoryTransactionListItem = {
  inventoryTransactionId: string;
  storeId: string;
  storeName: string;
  productId: string;
  productCode: string;
  productName: string;
  transactionType: InventoryTransactionType;
  quantityChange: number;
  quantityAfter: number;
  occurredAt: string;
  referenceNumber?: string | null;
  note?: string | null;
  createdAt: string;
  createdBy: string;
};

export type InventoryListItem = {
  inventoryId: string;
  productId: string;
  productCode: string;
  productName: string;
  productCategoryCode: string;
  storeId: string;
  storeCode: string;
  storeName: string;
  areaName: string;
  quantity: number;
  updatedAt: string;
};

export type InventoryTransactionType =
  | 1  // PurchaseReceive
  | 2  // ShipmentOut
  | 3  // ShipmentIn
  | 4  // Disposal
  | 5  // Adjustment
  | 6  // InitialStock
  | 11 // Sale
  | 12; // Return

export const inventoryTransactionTypeLabels: Record<InventoryTransactionType, string> = {
  1: "発注入荷",
  2: "出荷（出庫）",
  3: "入荷（入庫）",
  4: "廃棄",
  5: "棚卸調整",
  6: "初期在庫",
  11: "販売",
  12: "返品",
};
