import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getStayCareLog, logCareEntry, type LogCareEntryInput } from "./api";

const PAGE_SIZE = 20;

export const operationsKeys = {
  all: ["operations"] as const,
  careLog: (reservationId: string) => ["operations", "care-log", reservationId] as const,
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
