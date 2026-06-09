import type { Role } from "./roles";

const STORAGE_KEY = "pethotel.access_token";

/** Claims que o front consome do JWT. Nomes seguem os claim types comuns do ASP.NET. */
export interface AuthClaims {
  sub: string;
  email?: string;
  tenantId: string;
  roles: Role[];
  exp: number;
}

export function getStoredToken(): string | null {
  return localStorage.getItem(STORAGE_KEY);
}

export function setStoredToken(token: string | null): void {
  if (token) localStorage.setItem(STORAGE_KEY, token);
  else localStorage.removeItem(STORAGE_KEY);
}

function base64UrlDecode(input: string): string {
  const pad = input.length % 4 === 0 ? "" : "=".repeat(4 - (input.length % 4));
  const base64 = (input + pad).replace(/-/g, "+").replace(/_/g, "/");
  return decodeURIComponent(
    atob(base64)
      .split("")
      .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
      .join(""),
  );
}

const ROLE_CLAIMS = [
  "role",
  "roles",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
];
const TENANT_CLAIMS = ["tenantId", "tenant_id", "tid"];

/** Decodifica o JWT sem validar assinatura — validação é responsabilidade do backend. */
export function parseClaims(token: string): AuthClaims | null {
  try {
    const [, payload] = token.split(".");
    if (!payload) return null;
    const raw = JSON.parse(base64UrlDecode(payload)) as Record<string, unknown>;

    const roleValue = ROLE_CLAIMS.map((k) => raw[k]).find((v) => v != null);
    const roles = (Array.isArray(roleValue) ? roleValue : roleValue ? [roleValue] : []) as Role[];
    const tenantId = String(TENANT_CLAIMS.map((k) => raw[k]).find((v) => v != null) ?? "");

    return {
      sub: String(raw.sub ?? raw.nameid ?? ""),
      email: raw.email ? String(raw.email) : undefined,
      tenantId,
      roles,
      exp: Number(raw.exp ?? 0),
    };
  } catch {
    return null;
  }
}

export function isExpired(claims: AuthClaims): boolean {
  return claims.exp > 0 && claims.exp * 1000 <= Date.now();
}
