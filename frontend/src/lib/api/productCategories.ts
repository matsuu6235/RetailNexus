import { apiGet, apiPost, apiPut } from "./client";
import type { ProductCategory } from "@/types/productCategories";

const MAX_PAGE_SIZE = 200;

export type ProductCategoryListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: ProductCategory[];
};

export type CreateProductCategoryRequest = {
  productCategoryCd: string;
  categoryAbbreviation: string;
  productCategoryName: string;
  isActive: boolean;
};

export type UpdateProductCategoryRequest = CreateProductCategoryRequest;

export type ReorderProductCategoriesRequest = {
  productCategoryIds: string[];
};

export async function getProductCategories(
  page = 1,
  pageSize = 20,
  search?: {
    productCategoryCd?: string;
    productCategoryName?: string;
    isActive?: "all" | "active" | "inactive";
  }
): Promise<ProductCategoryListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (search?.productCategoryCd?.trim()) {
    params.set("productCategoryCd", search.productCategoryCd.trim());
  }

  if (search?.productCategoryName?.trim()) {
    params.set("productCategoryName", search.productCategoryName.trim());
  }

  if (search?.isActive === "active") {
    params.set("isActive", "true");
  }

  if (search?.isActive === "inactive") {
    params.set("isActive", "false");
  }

  return apiGet<ProductCategoryListResponse>(`/api/productcategories?${params.toString()}`);
}

export async function getAllProductCategories(search?: {
  productCategoryCd?: string;
  productCategoryName?: string;
  isActive?: "all" | "active" | "inactive";
}): Promise<ProductCategory[]> {
  let page = 1;
  let total = 0;
  const items: ProductCategory[] = [];

  do {
    const response = await getProductCategories(page, MAX_PAGE_SIZE, search);
    total = response.total;
    items.push(...response.items);
    page += 1;
  } while (items.length < total);

  return items;
}

export async function getProductCategoryById(id: string) {
  return apiGet<ProductCategory>(`/api/productcategories/${id}`);
}

export async function createProductCategory(body: CreateProductCategoryRequest) {
  return apiPost<CreateProductCategoryRequest, ProductCategory>("/api/productcategories", body);
}

export async function updateProductCategory(id: string, body: UpdateProductCategoryRequest) {
  return apiPut<UpdateProductCategoryRequest, ProductCategory>(`/api/productcategories/${id}`, body);
}

export async function reorderProductCategories(productCategoryIds: string[]) {
  return apiPut<ReorderProductCategoriesRequest, ProductCategory[]>(
    "/api/productcategories/display-order",
    { productCategoryIds }
  );
}