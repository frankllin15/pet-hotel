import { useQuery } from "@tanstack/react-query";
import { listUsers } from "./api";

export const userKeys = {
  all: ["users"] as const,
  list: () => ["users", "list"] as const,
};

/**
 * Diretório de usuários do hotel, cacheado por bastante tempo (muda raramente). Usado para
 * resolver id → nome em campos de auditoria (quem registrou/aplicou/reportou).
 */
export function useUsers() {
  return useQuery({
    queryKey: userKeys.list(),
    queryFn: listUsers,
    staleTime: 1 * 60 * 1000,
  });
}
