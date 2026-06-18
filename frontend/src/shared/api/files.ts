import { getStoredToken, setStoredToken } from "@/shared/auth/token";
import { AUTH_SIGNOUT_EVENT } from "@/shared/auth/auth-context";
import { newCorrelationId } from "@/shared/lib/correlation";
import { setCorrelationContext } from "@/shared/observability/sentry";
import { toApiError } from "@/shared/lib/problem-details";

/**
 * Acesso a arquivos (upload multipart e download autenticado de imagens). Não passa
 * pelo openapi-fetch porque o corpo é binário/multipart; reaproveita token, correlation
 * id e o tratamento de 401 do client gerado (docs/08).
 */
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

/** Limites espelhados do backend (ImageUploads) — validação client-side é só UX. */
export const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"] as const;
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;

function authHeaders(): Record<string, string> {
  const correlationId = newCorrelationId();
  setCorrelationContext(correlationId); // casa com o tracing do backend (Sentry ↔ Serilog/OTel)
  const headers: Record<string, string> = { "X-Correlation-Id": correlationId };
  const token = getStoredToken();
  if (token) headers.Authorization = `Bearer ${token}`;
  return headers;
}

function onUnauthorized(status: number): void {
  if (status === 401) {
    setStoredToken(null);
    window.dispatchEvent(new Event(AUTH_SIGNOUT_EVENT));
  }
}

async function failFrom(response: Response): Promise<never> {
  const body = await response.json().catch(() => null);
  throw toApiError(response.status, body);
}

/** POST multipart com o campo `file`; devolve o corpo JSON tipado. */
export async function uploadFile<T>(path: string, file: File): Promise<T> {
  const form = new FormData();
  form.append("file", file);

  const response = await fetch(`${baseUrl}${path}`, {
    method: "POST",
    headers: authHeaders(), // sem Content-Type: o browser define o boundary do multipart
    body: form,
  });

  onUnauthorized(response.status);
  if (!response.ok) await failFrom(response);
  return (await response.json()) as T;
}

export async function deleteResource(path: string): Promise<void> {
  const response = await fetch(`${baseUrl}${path}`, { method: "DELETE", headers: authHeaders() });
  onUnauthorized(response.status);
  if (!response.ok) await failFrom(response);
}

/**
 * Baixa um arquivo protegido e devolve um object URL. O chamador é dono da URL e deve
 * revogá-la (URL.revokeObjectURL) quando não precisar mais — ver <AuthImage>.
 */
export async function fetchObjectUrl(path: string): Promise<string> {
  const response = await fetch(`${baseUrl}${path}`, { headers: authHeaders() });
  onUnauthorized(response.status);
  if (!response.ok) await failFrom(response);
  return URL.createObjectURL(await response.blob());
}
