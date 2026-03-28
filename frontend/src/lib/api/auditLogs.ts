import { apiGet } from "./client";
import type { AuditLog } from "@/types/auditLogs";

export type AuditLogListResponse = {
  total: number;
  page: number;
  pageSize: number;
  items: AuditLog[];
};

export type AuditLogSearchParams = {
  from?: string;
  to?: string;
  userName?: string;
  action?: string;
  entityName?: string;
};

export async function getAuditLogs(
  page = 1,
  pageSize = 50,
  searchParams?: AuditLogSearchParams
): Promise<AuditLogListResponse> {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  if (searchParams?.from) params.set("from", searchParams.from);
  if (searchParams?.to) params.set("to", searchParams.to);
  if (searchParams?.userName?.trim()) params.set("userName", searchParams.userName.trim());
  if (searchParams?.action && searchParams.action !== "all") params.set("action", searchParams.action);
  if (searchParams?.entityName && searchParams.entityName !== "all") params.set("entityName", searchParams.entityName);

  return apiGet<AuditLogListResponse>(`/api/auditlogs?${params.toString()}`);
}
