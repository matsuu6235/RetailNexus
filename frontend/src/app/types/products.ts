export type Product = {
  id: string;
  productCode: string;
  productName: string;
  janCode: string;
  categoryCode: string;
  categoryName?: string | null;
  price: number;
  cost: number;
  isActive: boolean;
  updatedAt: string;
  createdAt: string;
};
