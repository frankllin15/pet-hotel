import { apiClient, unwrap } from "@/shared/api/client";
import { deleteResource, uploadFile } from "@/shared/api/files";
import type { components } from "@/shared/api/schema";

// Tipos do contrato OpenAPI — nunca digitados à mão (docs/08).
export type TutorDto = components["schemas"]["TutorDto"];
export type PetDto = components["schemas"]["PetDto"];
export type PetPhotoResponse = components["schemas"]["PetPhotoResponse"];
export type Species = components["schemas"]["Species"];
export type RegisterTutorBody = components["schemas"]["RegisterTutor"];
export type RegisterPetBody = components["schemas"]["RegisterPet"];
export type UpdateTutorBody = components["schemas"]["UpdateTutor"];
export type UpdatePetBody = components["schemas"]["UpdatePet"];
type TutorPage = components["schemas"]["CursorPageOfTutorDto"];
type PetPage = components["schemas"]["CursorPageOfPetDto"];

export interface ListTutorsParams {
  search?: string;
  cursor?: string;
  limit?: number;
}

export interface ListPetsParams {
  search?: string;
  tutorId?: string;
  cursor?: string;
  limit?: number;
}

export async function listTutors(params: ListTutorsParams): Promise<TutorPage> {
  return unwrap(await apiClient.GET("/v1/tutors", { params: { query: params } }));
}

export async function getTutor(id: string): Promise<TutorDto> {
  return unwrap(await apiClient.GET("/v1/tutors/{id}", { params: { path: { id } } }));
}

export async function registerTutor(body: RegisterTutorBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/tutors", { body }));
}

export async function updateTutor(id: string, body: UpdateTutorBody): Promise<void> {
  unwrap(await apiClient.PUT("/v1/tutors/{id}", { params: { path: { id } }, body }));
}

export async function deleteTutor(id: string): Promise<void> {
  unwrap(await apiClient.DELETE("/v1/tutors/{id}", { params: { path: { id } } }));
}

export async function listPets(params: ListPetsParams): Promise<PetPage> {
  return unwrap(await apiClient.GET("/v1/pets", { params: { query: params } }));
}

export async function getPet(id: string): Promise<PetDto> {
  return unwrap(await apiClient.GET("/v1/pets/{id}", { params: { path: { id } } }));
}

export async function registerPet(body: RegisterPetBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/pets", { body }));
}

export async function updatePet(id: string, body: UpdatePetBody): Promise<void> {
  unwrap(await apiClient.PUT("/v1/pets/{id}", { params: { path: { id } }, body }));
}

export async function deletePet(id: string): Promise<void> {
  unwrap(await apiClient.DELETE("/v1/pets/{id}", { params: { path: { id } } }));
}

export async function uploadPetPhoto(id: string, file: File): Promise<PetPhotoResponse> {
  return uploadFile<PetPhotoResponse>(`/v1/pets/${id}/photo`, file);
}

export async function deletePetPhoto(id: string): Promise<void> {
  return deleteResource(`/v1/pets/${id}/photo`);
}
