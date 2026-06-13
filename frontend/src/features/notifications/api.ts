import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type OutboundMessageDto = components["schemas"]["OutboundMessageDto"];
export type CreateReportBody = components["schemas"]["CreateReport"];

export async function createReport(body: CreateReportBody): Promise<{ id: string }> {
  return unwrap(await apiClient.POST("/v1/reports", { body }));
}

export async function sendReport(id: string): Promise<void> {
  unwrap(await apiClient.POST("/v1/reports/{id}/send", { params: { path: { id } } }));
}

export async function getStayReports(reservationId: string): Promise<OutboundMessageDto[]> {
  return unwrap(await apiClient.GET("/v1/reservations/{reservationId}/reports", { params: { path: { reservationId } } }));
}

export async function getTutorReports(tutorId: string): Promise<OutboundMessageDto[]> {
  return unwrap(await apiClient.GET("/v1/tutors/{tutorId}/reports", { params: { path: { tutorId } } }));
}
