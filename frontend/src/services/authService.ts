const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL!;

type LoginRequest = {
  loginId: string;
  password: string;
};

type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  email: string;
  roles: string[];
  permissions: string[];
};

export async function login(body: LoginRequest): Promise<LoginResponse> {
  const res = await fetch(`${baseUrl}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    throw new Error(`Login failed: ${res.status}`);
  }

  const data = (await res.json()) as LoginResponse;

  if (typeof window !== "undefined") {
    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("loginUserName", body.loginId);
    localStorage.setItem("loginUserEmail", data.email);
    localStorage.setItem("roles", JSON.stringify(data.roles));
    localStorage.setItem("permissions", JSON.stringify(data.permissions));
  }

  return data;
}

export function getLoggedInUserName(): string {
  if (typeof window === "undefined") return "";
  return localStorage.getItem("loginUserName") ?? "";
}

export function logout(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem("accessToken");
  localStorage.removeItem("loginUserName");
  localStorage.removeItem("loginUserEmail");
  localStorage.removeItem("roles");
  localStorage.removeItem("permissions");
}

export function getPermissions(): string[] {
  if (typeof window === "undefined") return [];
  try {
    return JSON.parse(localStorage.getItem("permissions") ?? "[]");
  } catch {
    return [];
  }
}

export function getRoles(): string[] {
  if (typeof window === "undefined") return [];
  try {
    return JSON.parse(localStorage.getItem("roles") ?? "[]");
  } catch {
    return [];
  }
}

export function hasPermission(permission: string): boolean {
  return getPermissions().includes(permission);
}