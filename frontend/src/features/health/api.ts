import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type PetHealthDto = components["schemas"]["PetHealthDto"];
export type VaccinationDto = components["schemas"]["VaccinationDto"];
export type VaccineType = components["schemas"]["VaccineType"];
export type RegisterVaccinationBody = components["schemas"]["RegisterVaccinationRequest"];

export async function getPetHealth(petId: string): Promise<PetHealthDto> {
  return unwrap(await apiClient.GET("/v1/pets/{petId}/health", { params: { path: { petId } } }));
}

export async function registerVaccination(
  petId: string,
  body: RegisterVaccinationBody,
): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/pets/{petId}/vaccinations", { params: { path: { petId } }, body }),
  );
}
