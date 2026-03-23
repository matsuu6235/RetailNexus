const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL!;

type LoginRequest = {
  loginId: string;
  password: string;
};

type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  email: string;
  role: string;
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
}