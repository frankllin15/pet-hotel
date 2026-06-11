import { type ReactNode } from "react";
import { PageHeader } from "../page-header";

/**
 * Arquétipo Detalhe (docs/08 §3): cabeçalho + abas + painel lateral.
 * Telas: ficha do pet, ficha da reserva.
 */
export function DetailPage({
  title,
  description,
  actions,
  tabs,
  sidePanel,
  children,
}: {
  title: ReactNode;
  description?: ReactNode;
  actions?: ReactNode;
  /** Navegação por abas, opcional. */
  tabs?: ReactNode;
  /** Painel lateral (resumo, metadados). */
  sidePanel?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="rise-in space-y-6">
      <PageHeader title={title} description={description} actions={actions} />
      {tabs && <div className="border-b">{tabs}</div>}
      <div className="grid gap-6 lg:grid-cols-[1fr_20rem]">
        <div className="min-w-0">{children}</div>
        {sidePanel && <aside className="space-y-4">{sidePanel}</aside>}
      </div>
    </div>
  );
}
