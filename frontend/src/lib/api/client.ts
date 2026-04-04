import { httpError } from "@/lib/messages";

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

if (!baseUrl) {
  throw new Error("NEXT_PUBLIC_API_BASE_URL is not set.");
}

function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("accessToken");
}

async function handleJsonResponse<T>(path: string, res: Response, method: string): Promise<T> {
  if (!res.ok) {
    if (res.status === 401) {
      throw new Error(httpError.sessionExpired);
    }
    if (res.status === 403) {
      throw new Error(httpError.forbidden);
    }
    if (res.status >= 500) {
      throw new Error(httpError.serverError);
    }
    const text = await res.text().catch(() => "");
    try {
      const json = JSON.parse(text);
      // ミドルウェアの { "message": "..." } 形式
      if (typeof json.message === "string") throw new Error(json.message);
      // FluentValidation の { "FieldName": ["message"] } 形式
      const source = json.errors ?? json;
      const messages = (Object.values(source) as unknown[]).flat().filter(Boolean) as string[];
      if (messages.length > 0) throw new Error(messages.join("\n"));
    } catch (e) {
      if (e instanceof Error && e.message !== text) throw e;
    }
    throw new Error(text || httpError.unknownError(res.status));
  }
  if (res.status === 204) {
    // No Content
    return undefined as unknown as T;
  }
  return (await res.json()) as T;
}

export async function apiGet<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getAccessToken();

  const res = await fetch(`${baseUrl}${path}`, {
    method: "GET",
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
    ...init,
  });

  return handleJsonResponse<T>(path, res, "GET");
}

export async function apiPost<TRequest, TResponse>(
  path: string,
  body: TRequest,
  init?: RequestInit
): Promise<TResponse> {
  const token = getAccessToken();

  const res = await fetch(`${baseUrl}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
    body: JSON.stringify(body),
    ...init,
  });

  return handleJsonResponse<TResponse>(path, res, "POST");
}

export async function apiPut<TRequest, TResponse>(
  path: string,
  body: TRequest,
  init?: RequestInit
): Promise<TResponse> {
  const token = getAccessToken();

  const res = await fetch(`${baseUrl}${path}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
    body: JSON.stringify(body),
    ...init,
  });

  return handleJsonResponse<TResponse>(path, res, "PUT");
}