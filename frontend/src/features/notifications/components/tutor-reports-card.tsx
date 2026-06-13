import { Copy } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { useTutorReports } from "../queries";

/** Histórico de relatórios enviados a um tutor (somente leitura + copiar). */
export function TutorReportsCard({ tutorId }: { tutorId: string }) {
  const query = useTutorReports(tutorId);

  return (
    <AsyncBoundary
      query={query}
      isEmpty={(data) => data.length === 0}
      empty={<p className="text-sm text-muted-foreground">Nenhum relatório enviado a este tutor.</p>}
    >
      {(reports) => (
        <div className="space-y-2">
          {reports.map((r) => (
            <div key={r.id} className="rounded-lg border bg-card p-3 text-sm shadow-card">
              <div className="flex flex-wrap items-center gap-2">
                <span className="font-medium">{r.title}</span>
                {r.status === "Sent" ? (
                  <Badge variant="success">Enviado</Badge>
                ) : (
                  <Badge variant="secondary">Rascunho</Badge>
                )}
                <span className="text-xs text-muted-foreground">{formatDate(r.reportDate)}</span>
              </div>
              <p className="mt-1 whitespace-pre-line text-muted-foreground">{r.content}</p>
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="mt-2"
                onClick={() => navigator.clipboard?.writeText(r.content)}
              >
                <Copy /> Copiar
              </Button>
            </div>
          ))}
        </div>
      )}
    </AsyncBoundary>
  );
}
