import { type ReactNode } from "react";
import { PageHeader } from "../page-header";

/**
 * Arquétipo Lista/Tabela (docs/08 §3): filtro fixo no topo, área de tabela,
 * ação primária no mesmo canto. Telas: reservas, pets, tutores, produtos.
 */
export function ListPage({
  title,
  description,
  primaryAction,
  filters,
  children,
}: {
  title: ReactNode;
  description?: ReactNode;
  /** Ação primária (ex.: "Nova reserva") — sempre no canto do cabeçalho. */
  primaryAction?: ReactNode;
  /** Barra de filtros fixa no topo. */
  filters?: ReactNode;
  /** Conteúdo da tabela (normalmente um <AsyncBoundary>). */
  children: ReactNode;
}) {
  return (
    <div className="space-y-6">
      <PageHeader title={title} description={description} actions={primaryAction} />
      {filters && <div className="flex flex-wrap items-center gap-3">{filters}</div>}
      <div>{children}</div>
    </div>
  );
}
