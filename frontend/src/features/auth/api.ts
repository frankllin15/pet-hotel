import { newCorrelationId } from "@/shared/lib/correlation";
import { toApiError } from "@/shared/lib/problem-details";
import type { LoginInput } from "./schemas";

/** Resposta do POST /v1/auth/login (AccessToken no backend). */
interface AccessToken {
  token: string;
  expiresAt: string;
}

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

/**
 * Autentica e devolve o JWT. Chamada manual (anônima) — quando o client OpenAPI
 * for gerado (`pnpm gen:api`), migrar para o apiClient tipado.
 */
export async function login(input: LoginInput): Promise<AccessToken> {
  const response = await fetch(`${baseUrl}/v1/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Correlation-Id": newCorrelationId(),
    },
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw toApiError(response.status, body);
  }
  return (await response.json()) as AccessToken;
}
