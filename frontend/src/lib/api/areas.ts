import { apiGet, apiPost, apiPut } from "./client";
import type { Area } from "@/types/areas";

const MAX_PAGE_SIZE = 200;

export type AreaListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: Area[];
};

export type CreateAreaRequest = {
  areaCd: string;
  areaName: string;
};

export type UpdateAreaRequest = CreateAreaRequest;

export type ReorderAreasRequest = {
  areaIds: string[];
};

export async function getAreas(
  page = 1,
  pageSize = 20,
  search?: {
    areaCd?: string;
    areaName?: string;
    isActive?: "all" | "active" | "inactive";
  }
): Promise<AreaListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (search?.areaCd?.trim()) params.set("areaCd", search.areaCd.trim());
  if (search?.areaName?.trim()) params.set("areaName", search.areaName.trim());
  if (search?.isActive === "active") params.set("isActive", "true");
  if (search?.isActive === "inactive") params.set("isActive", "false");

  return apiGet<AreaListResponse>(`/api/areas?${params.toString()}`);
}

export async function getAllAreas(search?: {
  areaCd?: string;
  areaName?: string;
  isActive?: "all" | "active" | "inactive";
}): Promise<Area[]> {
  let page = 1;
  let total = 0;
  const items: Area[] = [];

  do {
    const response = await getAreas(page, MAX_PAGE_SIZE, search);
    total = response.total;
    items.push(...response.items);
    page += 1;
  } while (items.length < total);

  return items;
}

export async function getAreaById(id: string): Promise<Area> {
  return apiGet<Area>(`/api/areas/${id}`);
}

export async function createArea(body: CreateAreaRequest): Promise<Area> {
  return apiPost<CreateAreaRequest, Area>("/api/areas", body);
}

export async function updateArea(id: string, body: UpdateAreaRequest): Promise<Area> {
  return apiPut<UpdateAreaRequest, Area>(`/api/areas/${id}`, body);
}

export async function reorderAreas(areaIds: string[]): Promise<Area[]> {
  return apiPut<ReorderAreasRequest, Area[]>("/api/areas/display-order", { areaIds });
}

export async function changeAreaActivation(id: string, isActive: boolean): Promise<void> {
  return apiPut<{ isActive: boolean }, void>(`/api/areas/${id}/activation`, { isActive });
}