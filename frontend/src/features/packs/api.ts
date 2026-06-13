import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type PackDto = components["schemas"]["PackDto"];
export type PackSummaryDto = components["schemas"]["PackSummaryDto"];
export type PackMemberDto = components["schemas"]["PackMemberDto"];
export type CreatePackBody = components["schemas"]["CreatePack"];
export type UpdatePackBody = components["schemas"]["UpdatePack"];

export async function listPacks(): Promise<PackSummaryDto[]> {
  return unwrap(await apiClient.GET("/v1/packs"));
}

export async function getPack(id: string): Promise<PackDto> {
  return unwrap(await apiClient.GET("/v1/packs/{id}", { params: { path: { id } } }));
}

export async function createPack(body: CreatePackBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/packs", { body }));
}

export async function updatePack(id: string, body: UpdatePackBody): Promise<void> {
  unwrap(await apiClient.PUT("/v1/packs/{id}", { params: { path: { id } }, body }));
}

export async function deletePack(id: string): Promise<void> {
  unwrap(await apiClient.DELETE("/v1/packs/{id}", { params: { path: { id } } }));
}
