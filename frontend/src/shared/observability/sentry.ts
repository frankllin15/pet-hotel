import * as Sentry from "@sentry/react";

/**
 * Observabilidade do front (docs/08 §Testes e observabilidade): rastreamento de
 * erro + Web Vitals + Session Replay via Sentry, propagando o `correlation id`
 * para casar com o tracing do backend (Serilog/OpenTelemetry).
 *
 * Tudo é "gated" pela presença de `VITE_SENTRY_DSN`: sem DSN (dev/local), `init`
 * é no-op e as demais funções não fazem nada — nenhum dado sai do navegador.
 */

const dsn = import.meta.env.VITE_SENTRY_DSN as string | undefined;

let initialized = false;

/** Lê uma taxa de amostragem 0..1 do env, com fallback. */
function sampleRate(value: string | undefined, fallback: number): number {
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed >= 0 && parsed <= 1 ? parsed : fallback;
}

/**
 * Inicializa o Sentry. Chame uma única vez, o mais cedo possível (antes do render),
 * para capturar erros de carregamento. No-op se `VITE_SENTRY_DSN` não estiver definido.
 */
export function initObservability(): void {
  if (initialized || !dsn) return;
  initialized = true;

  Sentry.init({
    dsn,
    environment: (import.meta.env.VITE_SENTRY_ENVIRONMENT as string) ?? import.meta.env.MODE,
    release: import.meta.env.VITE_APP_VERSION as string | undefined,

    integrations: [
      // Web Vitals + tracing de navegação e fetch (distribui sentry-trace/baggage).
      Sentry.browserTracingIntegration(),
      // Session Replay com privacidade por padrão (LGPD): mascara TODO texto de
      // entrada/saída e bloqueia mídia — não vaza dados de tutores/pets.
      Sentry.replayIntegration({
        maskAllText: true,
        maskAllInputs: true,
        blockAllMedia: true,
      }),
    ],

    // Amostragem ajustável por ambiente (sem DSN nada disso roda).
    tracesSampleRate: sampleRate(import.meta.env.VITE_SENTRY_TRACES_SAMPLE_RATE, 0.1),
    replaysSessionSampleRate: sampleRate(import.meta.env.VITE_SENTRY_REPLAYS_SAMPLE_RATE, 0.1),
    // Em sessões que terminam em erro, sempre grava o replay para diagnóstico.
    replaysOnErrorSampleRate: 1.0,
    enableLogs: true
  });
}

/**
 * Marca o `correlation id` da requisição atual no escopo do Sentry, para que os
 * eventos/erros subsequentes carreguem o mesmo id usado no header X-Correlation-Id
 * e casem com o tracing do backend. No-op se o Sentry não foi inicializado.
 */
export function setCorrelationContext(correlationId: string): void {
  if (!initialized) return;
  Sentry.getCurrentScope().setTag("correlation_id", correlationId);
}

/**
 * Reporta uma exceção capturada por um error boundary, com a tag da feature.
 * No-op se o Sentry não foi inicializado (cai no console pelo chamador).
 */
export function captureBoundaryError(
  error: Error,
  feature: string,
  componentStack?: string | null,
): void {
  if (!initialized) return;
  Sentry.withScope((scope) => {
    scope.setTag("feature", feature);
    if (componentStack) {
      scope.setContext("react", { componentStack });
    }
    Sentry.captureException(error);
  });
}

/**
 * Reporta uma falha de API (resposta 5xx ou erro de rede/backend inacessível) como
 * evento no Sentry. Falhas "de negócio" (4xx: 400/401/404/409) NÃO passam por aqui —
 * são esperadas e tratadas na UI, virariam ruído. A `url` é só o pathname (sem query)
 * para não vazar termos de busca/PII. No-op se o Sentry não foi inicializado.
 */
export function captureApiError(
  error: unknown,
  context: { status?: number; method?: string; path?: string; correlationId?: string | null },
): void {
  if (!initialized) return;
  Sentry.withScope((scope) => {
    scope.setTag("api_error", true);
    if (context.status) scope.setTag("http_status", String(context.status));
    if (context.correlationId) scope.setTag("correlation_id", context.correlationId);
    scope.setContext("request", {
      method: context.method,
      path: context.path,
      status: context.status,
    });
    // Mensagem estável (método + endpoint + status) para o Sentry agrupar bem.
    const summary = `API ${context.status ?? "network"} ${context.method ?? ""} ${context.path ?? ""}`.trim();
    Sentry.captureException(error instanceof Error ? error : new Error(summary));
  });
}

/** Associa o usuário autenticado aos eventos (e limpa no logout). */
export function setObservabilityUser(user: { id: string; tenantId?: string } | null): void {
  if (!initialized) return;
  if (!user) {
    Sentry.setUser(null);
    return;
  }
  Sentry.setUser({ id: user.id });
  if (user.tenantId) {
    Sentry.setTag("tenant_id", user.tenantId);
  }
}
