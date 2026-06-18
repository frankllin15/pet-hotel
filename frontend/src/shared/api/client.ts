import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./schema";
import { getStoredToken, setStoredToken } from "@/shared/auth/token";
import { AUTH_SIGNOUT_EVENT } from "@/shared/auth/auth-context";
import { newCorrelationId } from "@/shared/lib/correlation";
import { setCorrelationContext, captureApiError } from "@/shared/observability/sentry";
import { toApiError } from "@/shared/lib/problem-details";

/** Extrai só o pathname da URL da request (sem query, para não vazar busca/PII no Sentry). */
function requestPath(request: Request): string {
  try {
    return new URL(request.url).pathname;
  } catch {
    return request.url;
  }
}

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
    // Casa o tracing do front com o do backend (Sentry ↔ Serilog/OTel).
    setCorrelationContext(request.headers.get("X-Correlation-Id")!);
    return request;
  },
  async onResponse({ request, response }) {
    // 401 -> sessão inválida/expirada: limpa e avisa o AuthProvider.
    if (response.status === 401) {
      setStoredToken(null);
      window.dispatchEvent(new Event(AUTH_SIGNOUT_EVENT));
    }
    // 5xx -> falha do servidor (não é erro de negócio): reporta ao Sentry.
    if (response.status >= 500) {
      captureApiError(`API ${response.status} ${request.method} ${requestPath(request)}`, {
        status: response.status,
        method: request.method,
        path: requestPath(request),
        correlationId: request.headers.get("X-Correlation-Id"),
      });
    }
    return response;
  },
  // Erro de rede / backend inacessível (o fetch rejeita, sem resposta HTTP).
  onError({ request, error }) {
    captureApiError(error, {
      method: request.method,
      path: requestPath(request),
      correlationId: request.headers.get("X-Correlation-Id"),
    });
    // não retorna nada: deixa o erro original propagar para o React Query.
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
