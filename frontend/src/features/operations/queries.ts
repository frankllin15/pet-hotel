import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addCareEntryPhoto,
  deleteCareEntryPhoto,
  getStayCareLog,
  getStayIncidents,
  getStayMedications,
  logCareEntry,
  recordMedication,
  reportIncident,
  type IncidentSeverity,
  type LogCareEntryInput,
} from "./api";

const PAGE_SIZE = 20;

export const operationsKeys = {
  all: ["operations"] as const,
  careLog: (reservationId: string) => ["operations", "care-log", reservationId] as const,
  medications: (reservationId: string) => ["operations", "medications", reservationId] as const,
  incidents: (reservationId: string) => ["operations", "incidents", reservationId] as const,
};

export function useStayCareLog(reservationId: string) {
  return useInfiniteQuery({
    queryKey: operationsKeys.careLog(reservationId),
    queryFn: ({ pageParam }) => getStayCareLog(reservationId, { cursor: pageParam, limit: PAGE_SIZE }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
    enabled: !!reservationId,
  });
}

export function useLogCareEntry(reservationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: LogCareEntryInput) => logCareEntry(reservationId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operationsKeys.careLog(reservationId) }),
  });
}

/** Upload/remoção de foto numa ocorrência, invalidando a timeline. */
export function useCareEntryPhotos(reservationId: string) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: operationsKeys.careLog(reservationId) });
  const upload = useMutation({
    mutationFn: ({ entryId, file }: { entryId: string; file: File }) => addCareEntryPhoto(entryId, file),
    onSuccess: invalidate,
  });
  const remove = useMutation({
    mutationFn: ({ entryId, key }: { entryId: string; key: string }) => deleteCareEntryPhoto(entryId, key),
    onSuccess: invalidate,
  });
  return { upload, remove };
}

export function useStayMedications(reservationId: string) {
  return useQuery({
    queryKey: operationsKeys.medications(reservationId),
    queryFn: () => getStayMedications(reservationId),
    enabled: !!reservationId,
  });
}

export function useRecordMedication(reservationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: { drug: string; dose: string }) => recordMedication(reservationId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operationsKeys.medications(reservationId) }),
  });
}

export function useStayIncidents(reservationId: string) {
  return useQuery({
    queryKey: operationsKeys.incidents(reservationId),
    queryFn: () => getStayIncidents(reservationId),
    enabled: !!reservationId,
  });
}

export function useReportIncident(reservationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: { severity: IncidentSeverity; description: string }) => reportIncident(reservationId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operationsKeys.incidents(reservationId) }),
  });
}
