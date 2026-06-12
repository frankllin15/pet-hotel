import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type AccommodationDto = components["schemas"]["AccommodationDto"];
export type ReservationDto = components["schemas"]["ReservationDto"];
export type OccupancyEntryDto = components["schemas"]["OccupancyEntryDto"];
export type CreateReservationBody = components["schemas"]["CreateReservation"];
export type ArrivalStateInput = components["schemas"]["ArrivalStateInput"];

export async function listAccommodations(): Promise<AccommodationDto[]> {
  return unwrap(await apiClient.GET("/v1/accommodations"));
}

export async function createAccommodation(name: string): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/accommodations", { body: { name } }));
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
