import { type ReactNode } from "react";
import { PageHeader } from "../page-header";

/**
 * Arquétipo Dashboard (docs/08 §3): grid de cards com anatomia única.
 * Tela: painel da gerência.
 */
export function DashboardPage({
  title,
  description,
  actions,
  children,
}: {
  title: ReactNode;
  description?: ReactNode;
  actions?: ReactNode;
  /** Cards do painel — use <DashboardGrid>. */
  children: ReactNode;
}) {
  return (
    <div className="space-y-6">
      <PageHeader title={title} description={description} actions={actions} />
      {children}
    </div>
  );
}

export function DashboardGrid({ children }: { children: ReactNode }) {
  return <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">{children}</div>;
}
