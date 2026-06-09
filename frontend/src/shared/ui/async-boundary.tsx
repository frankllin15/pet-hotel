import { type ReactNode } from "react";
import type { UseQueryResult } from "@tanstack/react-query";
import { AlertTriangle } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "./button";
import { Spinner } from "./spinner";

/**
 * Wrapper único sobre o resultado do Query (docs/08 §Estados padronizados):
 * loading / erro / vazio / sucesso iguais em todo lugar. Nenhuma tela escreve
 * seu próprio spinner ou estado de erro.
 */
interface AsyncBoundaryProps<T> {
  query: UseQueryResult<T>;
  children: (data: T) => ReactNode;
  /** Conteúdo do estado vazio. Default: mensagem genérica. */
  empty?: ReactNode;
  /** Como decidir se `data` está vazio. Default: array de tamanho 0. */
  isEmpty?: (data: T) => boolean;
  loading?: ReactNode;
}

export function AsyncBoundary<T>({
  query,
  children,
  empty,
  isEmpty = defaultIsEmpty,
  loading,
}: AsyncBoundaryProps<T>) {
  if (query.isPending) {
    return loading ?? <LoadingState />;
  }
  if (query.isError) {
    return <ErrorState error={query.error} onRetry={() => query.refetch()} />;
  }
  if (isEmpty(query.data)) {
    return <>{empty ?? <EmptyState />}</>;
  }
  return <>{children(query.data)}</>;
}

function defaultIsEmpty(data: unknown): boolean {
  return Array.isArray(data) && data.length === 0;
}

function LoadingState() {
  return (
    <div className="flex min-h-40 items-center justify-center">
      <Spinner />
    </div>
  );
}

function EmptyState() {
  return (
    <div className="flex min-h-40 flex-col items-center justify-center gap-1 text-center text-muted-foreground">
      <p className="text-sm">Nada por aqui ainda.</p>
    </div>
  );
}

function ErrorState({ error, onRetry }: { error: unknown; onRetry: () => void }) {
  const message =
    error instanceof ApiError
      ? error.message
      : "Não foi possível carregar os dados.";
  return (
    <div className="flex min-h-40 flex-col items-center justify-center gap-3 text-center">
      <AlertTriangle className="size-6 text-destructive" />
      <p className="text-sm text-muted-foreground">{message}</p>
      <Button variant="outline" size="sm" onClick={onRetry}>
        Tentar de novo
      </Button>
    </div>
  );
}
