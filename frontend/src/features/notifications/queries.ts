import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createReport, getStayReports, getTutorReports, sendReport, type CreateReportBody } from "./api";

export const notificationKeys = {
  all: ["notifications"] as const,
  stayReports: (reservationId: string) => ["notifications", "stay-reports", reservationId] as const,
  tutorReports: (tutorId: string) => ["notifications", "tutor-reports", tutorId] as const,
};

export function useStayReports(reservationId: string) {
  return useQuery({
    queryKey: notificationKeys.stayReports(reservationId),
    queryFn: () => getStayReports(reservationId),
    enabled: !!reservationId,
  });
}

export function useTutorReports(tutorId: string) {
  return useQuery({
    queryKey: notificationKeys.tutorReports(tutorId),
    queryFn: () => getTutorReports(tutorId),
    enabled: !!tutorId,
  });
}

export function useCreateReport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateReportBody) => createReport(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  });
}

export function useSendReport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sendReport(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  });
}
