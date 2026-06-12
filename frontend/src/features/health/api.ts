import { apiClient, unwrap } from "@/shared/api/client";
import { deleteResource, uploadFile } from "@/shared/api/files";
import type { components } from "@/shared/api/schema";

export type PetHealthDto = components["schemas"]["PetHealthDto"];
export type VaccinationPhotoResponse = components["schemas"]["VaccinationPhotoResponse"];
export type VaccinationDto = components["schemas"]["VaccinationDto"];
export type VaccineType = components["schemas"]["VaccineType"];
export type RegisterVaccinationBody = components["schemas"]["RegisterVaccinationRequest"];
export type ParasiteTreatmentDto = components["schemas"]["ParasiteTreatmentDto"];
export type RegisterParasiteTreatmentBody = components["schemas"]["RegisterParasiteTreatmentRequest"];
export type SetVetContactBody = components["schemas"]["SetVetContactRequest"];

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

export async function registerParasiteTreatment(
  petId: string,
  body: RegisterParasiteTreatmentBody,
): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/pets/{petId}/parasite-treatments", { params: { path: { petId } }, body }),
  );
}

export async function setVetContact(petId: string, body: SetVetContactBody): Promise<void> {
  unwrap(await apiClient.PUT("/v1/pets/{petId}/vet-contact", { params: { path: { petId } }, body }));
}

export async function uploadVaccinationPhoto(
  petId: string,
  vaccinationId: string,
  file: File,
): Promise<VaccinationPhotoResponse> {
  return uploadFile<VaccinationPhotoResponse>(
    `/v1/pets/${petId}/vaccinations/${vaccinationId}/photo`,
    file,
  );
}

export async function deleteVaccinationPhoto(petId: string, vaccinationId: string): Promise<void> {
  return deleteResource(`/v1/pets/${petId}/vaccinations/${vaccinationId}/photo`);
}
