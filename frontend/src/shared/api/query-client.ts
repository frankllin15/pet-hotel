import { QueryClient } from "@tanstack/react-query";
import { ApiError } from "@/shared/lib/problem-details";

/**
 * Configuração central do TanStack Query (docs/08 §Estado, §Confiabilidade).
 * Retry com backoff exponencial; não insistir em erros de cliente (4xx).
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 5 * 60_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) => {
        if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
          return false;
        }
        return failureCount < 3;
      },
      retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 30_000),
    },
    mutations: {
      retry: false,
    },
  },
});
