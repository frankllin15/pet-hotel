import { Construction } from "lucide-react";
import { PageHeader } from "./page-header";

/**
 * Tela ainda não implementada — encaixa um arquétipo depois (docs/09).
 * Mantém a navegação coerente enquanto a feature não existe.
 */
export function PlaceholderPage({ title, phase }: { title: string; phase?: string }) {
  return (
    <div className="space-y-6">
      <PageHeader title={title} description={phase} />
      <div className="flex min-h-60 flex-col items-center justify-center gap-2 rounded-lg border border-dashed text-center text-muted-foreground">
        <Construction className="size-7" />
        <p className="text-sm">Em construção.</p>
      </div>
    </div>
  );
}
