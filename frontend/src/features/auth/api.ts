import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";
import type { LoginInput } from "./schemas";

/** Resposta do POST /v1/auth/login — tipo vem do contrato OpenAPI. */
export type AccessToken = components["schemas"]["AccessToken"];

/** Autentica e devolve o JWT via client tipado (gerado de /openapi/v1.json). */
export async function login(input: LoginInput): Promise<AccessToken> {
  return unwrap(await apiClient.POST("/v1/auth/login", { body: input }));
}
