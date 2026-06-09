import { type ReactNode } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { queryClient } from "@/shared/api/query-client";
import { AuthProvider } from "@/shared/auth/auth-context";
import { FeatureErrorBoundary } from "@/shared/ui/error-boundary";

/** Providers raiz do app (docs/08 §Ordem de implementação — Fundação). */
export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <FeatureErrorBoundary feature="root">
      <QueryClientProvider client={queryClient}>
        <AuthProvider>{children}</AuthProvider>
        {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
      </QueryClientProvider>
    </FeatureErrorBoundary>
  );
}
