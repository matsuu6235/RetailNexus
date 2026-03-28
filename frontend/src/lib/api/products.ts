import { apiGet, apiPost, apiPut } from "./client";
import type { Product } from "@/types/products";

export type ProductListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: Product[];
};

export type CreateProductRequest = {
  janCode: string;
  productName: string;
  price: number;
  cost: number;
  productCategoryCode: string;
};

export type UpdateProductRequest = {
  janCode: string;
  productName: string;
  price: number;
  cost: number;
  productCategoryCode: string;
};

export type ProductSearchParams = {
  productCode?: string;
  janCode?: string;
  productName?: string;
  productCategoryCode?: string;
  isActive?: "all" | "active" | "inactive";
};

export async function getProducts(
  page = 1,
  pageSize = 20,
  searchParams?: ProductSearchParams
): Promise<ProductListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.productCode?.trim()) {
    params.set("productCode", searchParams.productCode.trim());
  }

  if (searchParams?.janCode?.trim()) {
    params.set("janCode", searchParams.janCode.trim());
  }

  if (searchParams?.productName?.trim()) {
    params.set("productName", searchParams.productName.trim());
  }

  if (searchParams?.productCategoryCode?.trim()) {
    params.set("productCategoryCode", searchParams.productCategoryCode.trim());
  }

  if (searchParams?.isActive === "active") {
    params.set("isActive", "true");
  }

  if (searchParams?.isActive === "inactive") {
    params.set("isActive", "false");
  }

  return apiGet<ProductListResponse>(`/api/products?${params.toString()}`);
}

export async function getProductById(id: string): Promise<Product> {
  return apiGet<Product>(`/api/products/${id}`);
}

export async function createProduct(body: CreateProductRequest): Promise<Product> {
  return apiPost<CreateProductRequest, Product>("/api/products", body);
}

export async function updateProduct(id: string, body: UpdateProductRequest): Promise<Product> {
  return apiPut<UpdateProductRequest, Product>(`/api/products/${id}`, body);
}

export async function changeProductActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/products/${id}/activation`, { isActive });
}
