/**
 * Correlation id propagado em toda requisição (header X-Correlation-Id) para
 * casar o tracing do front com o do backend (docs/08 §Testes e observabilidade).
 */
export function newCorrelationId(): string {
  if (typeof crypto !== "undefined" && "randomUUID" in crypto) {
    return crypto.randomUUID();
  }
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
