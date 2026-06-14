import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type DashboardResponse = components["schemas"]["DashboardResponse"];
export type DayBoardDto = components["schemas"]["DayBoardDto"];
export type DayMedicationDto = components["schemas"]["DayMedicationDto"];
export type ExpiringVaccinationDto = components["schemas"]["ExpiringVaccinationDto"];

/** Painel do dia. `date` opcional (yyyy-MM-dd); default no backend = hoje. */
export async function getDashboard(date?: string): Promise<DashboardResponse> {
  return unwrap(
    await apiClient.GET("/v1/dashboard", {
      params: { query: date ? { date } : {} },
    }),
  );
}
