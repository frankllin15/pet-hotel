import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type CareLogEntryDto = components["schemas"]["CareLogEntryDto"];
export type CareLogEntryType = components["schemas"]["CareLogEntryType"];
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
