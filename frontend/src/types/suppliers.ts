export type Supplier = {
    supplierId: string;
    supplierCode: string;
    supplierName: string;
    phoneNumber?: string | null;
    email?: string | null;
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
    createdBy: string;
    updatedBy: string;
  };