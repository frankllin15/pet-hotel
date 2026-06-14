import { apiClient, unwrap } from "@/shared/api/client";
import { deleteResource, uploadFile } from "@/shared/api/files";
import type { components } from "@/shared/api/schema";

export type AccommodationDto = components["schemas"]["AccommodationDto"];
export type ReservationDto = components["schemas"]["ReservationDto"];
export type ArrivalPhotoResponse = components["schemas"]["ArrivalPhotoResponse"];
export type OccupancyEntryDto = components["schemas"]["OccupancyEntryDto"];
export type CreateReservationBody = components["schemas"]["CreateReservation"];
export type CreateAccommodationBody = components["schemas"]["CreateAccommodation"];
export type UpdateAccommodationBody = components["schemas"]["UpdateAccommodation"];
export type ArrivalStateInput = components["schemas"]["ArrivalStateInput"];
export type SharingCompatibilityDto = components["schemas"]["SharingCompatibilityDto"];
export type PetCompatibilityDto = components["schemas"]["PetCompatibilityDto"];

export interface SharingCompatibilityParams {
  accommodationId: string;
  checkIn: string;
  checkOut: string;
  petId: string;
}

export async function listAccommodations(): Promise<AccommodationDto[]> {
  return unwrap(await apiClient.GET("/v1/accommodations"));
}

export async function createAccommodation(body: CreateAccommodationBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/accommodations", { body }));
}

export async function updateAccommodation(id: string, body: UpdateAccommodationBody): Promise<void> {
  unwrap(await apiClient.PUT("/v1/accommodations/{id}", { params: { path: { id } }, body }));
}

export async function listReservations(status?: string): Promise<ReservationDto[]> {
  return unwrap(await apiClient.GET("/v1/reservations", { params: { query: { status } } }));
}

export async function getReservation(id: string): Promise<ReservationDto> {
  return unwrap(await apiClient.GET("/v1/reservations/{id}", { params: { path: { id } } }));
}

export async function createReservation(body: CreateReservationBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/reservations", { body }));
}

export async function confirmReservation(id: string): Promise<void> {
  unwrap(await apiClient.POST("/v1/reservations/{id}/confirm", { params: { path: { id } } }));
}

export async function checkInReservation(id: string, arrivalState?: ArrivalStateInput): Promise<void> {
  unwrap(
    await apiClient.POST("/v1/reservations/{id}/check-in", {
      params: { path: { id } },
      body: arrivalState,
    }),
  );
}

export async function checkOutReservation(id: string): Promise<void> {
  unwrap(await apiClient.POST("/v1/reservations/{id}/check-out", { params: { path: { id } } }));
}

export async function cancelReservation(id: string): Promise<void> {
  unwrap(await apiClient.POST("/v1/reservations/{id}/cancel", { params: { path: { id } } }));
}

export async function getOccupancy(from: string, to: string): Promise<OccupancyEntryDto[]> {
  return unwrap(await apiClient.GET("/v1/occupancy", { params: { query: { from, to } } }));
}

export async function getSharingCompatibility(params: SharingCompatibilityParams): Promise<SharingCompatibilityDto> {
  return unwrap(await apiClient.GET("/v1/reservations/compatibility", { params: { query: params } }));
}

export async function uploadArrivalPhoto(reservationId: string, file: File): Promise<ArrivalPhotoResponse> {
  return uploadFile<ArrivalPhotoResponse>(`/v1/reservations/${reservationId}/arrival-photos`, file);
}

export async function deleteArrivalPhoto(reservationId: string, key: string): Promise<void> {
  return deleteResource(`/v1/reservations/${reservationId}/arrival-photos?key=${encodeURIComponent(key)}`);
}
