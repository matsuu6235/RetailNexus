export type Product = {
  id: string;
  productCode: string;
  productName: string;
  janCode: string;
  productCategoryCode: string;
  categoryName?: string | null;
  price: number;
  cost: number;
  isActive: boolean;
  updatedAt: string;
  createdAt: string;
};
