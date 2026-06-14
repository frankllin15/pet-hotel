import { useQuery } from "@tanstack/react-query";
import { getDashboard } from "./api";

export const dashboardKeys = {
  all: ["dashboard"] as const,
  day: (date?: string) => ["dashboard", date ?? "today"] as const,
};

/** Painel do dia (chegadas/saídas, ocupação, medicações, alertas de vacina). */
export function useDashboard(date?: string) {
  return useQuery({
    queryKey: dashboardKeys.day(date),
    queryFn: () => getDashboard(date),
  });
}
