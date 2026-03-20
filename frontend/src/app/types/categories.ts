export type Category = {
  id: string;
  categoryCode: string;
  categoryName: string;
  parentCategoryId?: string | null;
  sortOrder: number;
  isActive: boolean;
  updatedAt: string;
  createdAt: string;
};
