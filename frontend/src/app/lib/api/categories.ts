import { apiGet } from "./client";
import type { Category } from "../../types/categories";

export async function getCategories(includeInactive = false): Promise<Category[]> {
  const params = new URLSearchParams();
  if (includeInactive) params.set("includeInactive", "true");
  const qs = params.toString();
  return apiGet<Category[]>(`/api/categories${qs ? `?${qs}` : ""}`);
}
