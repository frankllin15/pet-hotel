import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./schema";
import { getStoredToken, setStoredToken } from "@/shared/auth/token";
import { AUTH_SIGNOUT_EVENT } from "@/shared/auth/auth-context";
import { newCorrelationId } from "@/shared/lib/correlation";
import { toApiError } from "@/shared/lib/problem-details";

/**
 * Client de API type-safe gerado do OpenAPI (docs/08). Em dev, requests vão para
 * caminhos relativos (/v1/...) e o proxy do Vite encaminha ao backend.
 */
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

// Injeta Authorization e correlation id em toda requisição.
const authMiddleware: Middleware = {
  onRequest({ request }) {
    const token = getStoredToken();
    if (token) request.headers.set("Authorization", `Bearer ${token}`);
    if (!request.headers.has("X-Correlation-Id")) {
      request.headers.set("X-Correlation-Id", newCorrelationId());
    }
    return request;
  },
  async onResponse({ response }) {
    // 401 -> sessão inválida/expirada: limpa e avisa o AuthProvider.
    if (response.status === 401) {
      setStoredToken(null);
      window.dispatchEvent(new Event(AUTH_SIGNOUT_EVENT));
    }
    return response;
  },
};

export const apiClient = createClient<paths>({
  baseUrl,
  headers: { "Content-Type": "application/json" },
});

apiClient.use(authMiddleware);

/**
 * Lança ApiError quando `error` vier preenchido por uma chamada do openapi-fetch.
 * Use nas hooks de query/mutation para integrar com o mapeamento de ProblemDetails.
 */
export function unwrap<T>(result: {
  data?: T;
  error?: unknown;
  response: Response;
}): T {
  if (result.error !== undefined) {
    throw toApiError(result.response.status, result.error);
  }
  return result.data as T;
}
