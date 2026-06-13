import { apiClient, unwrap } from "@/shared/api/client";
import { deleteResource, uploadFile } from "@/shared/api/files";
import type { components } from "@/shared/api/schema";

export type CareLogEntryDto = components["schemas"]["CareLogEntryDto"];
export type CareLogEntryType = components["schemas"]["CareLogEntryType"];
export type MedicationDto = components["schemas"]["MedicationDto"];
export type IncidentDto = components["schemas"]["IncidentDto"];
export type IncidentSeverity = components["schemas"]["IncidentSeverity"];
export type CarePhotoResponse = components["schemas"]["CarePhotoResponse"];
type CareLogPage = components["schemas"]["CursorPageOfCareLogEntryDto"];

export interface CareLogParams {
  cursor?: string;
  limit?: number;
}

export interface LogCareEntryInput {
  type: CareLogEntryType;
  note?: string | null;
  occurredAt?: string | null;
}

export async function getStayCareLog(reservationId: string, params: CareLogParams = {}): Promise<CareLogPage> {
  return unwrap(
    await apiClient.GET("/v1/reservations/{reservationId}/care-log", {
      params: { path: { reservationId }, query: params },
    }),
  );
}

export async function logCareEntry(reservationId: string, input: LogCareEntryInput): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/reservations/{reservationId}/care-log", {
      params: { path: { reservationId } },
      body: { reservationId, type: input.type, note: input.note ?? null, occurredAt: input.occurredAt ?? null },
    }),
  );
}

export async function addCareEntryPhoto(entryId: string, file: File): Promise<CarePhotoResponse> {
  return uploadFile<CarePhotoResponse>(`/v1/care-log/${entryId}/photos`, file);
}

export async function deleteCareEntryPhoto(entryId: string, key: string): Promise<void> {
  return deleteResource(`/v1/care-log/${entryId}/photos?key=${encodeURIComponent(key)}`);
}

export async function getStayMedications(reservationId: string): Promise<MedicationDto[]> {
  return unwrap(await apiClient.GET("/v1/reservations/{reservationId}/medications", { params: { path: { reservationId } } }));
}

export async function recordMedication(
  reservationId: string,
  input: { drug: string; dose: string },
): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/reservations/{reservationId}/medications", {
      params: { path: { reservationId } },
      body: { reservationId, drug: input.drug, dose: input.dose, administeredAt: null },
    }),
  );
}

export async function getStayIncidents(reservationId: string): Promise<IncidentDto[]> {
  return unwrap(await apiClient.GET("/v1/reservations/{reservationId}/incidents", { params: { path: { reservationId } } }));
}

export async function reportIncident(
  reservationId: string,
  input: { severity: IncidentSeverity; description: string },
): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/reservations/{reservationId}/incidents", {
      params: { path: { reservationId } },
      body: { reservationId, severity: input.severity, description: input.description, occurredAt: null },
    }),
  );
}
