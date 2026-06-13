import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type DirectoryUser = components["schemas"]["UserSummaryResponse"];

/** Diretório leve (id + nome) do hotel para resolver autoria; acessível a qualquer autenticado. */
export async function listUsers(): Promise<DirectoryUser[]> {
  return unwrap(await apiClient.GET("/v1/users"));
}
