import { useState } from "react";
import { Copy, FileText, Send } from "lucide-react";
import { usePet } from "@/features/registry/queries";
import { useStayCareLog, useStayIncidents, useStayMedications } from "@/features/operations/queries";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Textarea } from "@/shared/ui/textarea";
import { useCreateReport, useSendReport, useStayReports } from "../queries";
import { buildReportText } from "../report-text";

const todayIso = () => new Date().toISOString().slice(0, 10);

/** Monta, salva e envia o relatório diário ao tutor a partir do diário da estadia. */
export function ReportPanel({ reservationId, petId }: { reservationId: string; petId: string }) {
  const pet = usePet(petId);
  const careLog = useStayCareLog(reservationId);
  const meds = useStayMedications(reservationId);
  const incidents = useStayIncidents(reservationId);
  const reports = useStayReports(reservationId);
  const create = useCreateReport();
  const send = useSendReport();

  const [date, setDate] = useState(todayIso());
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");

  const petName = pet.data?.name ?? "pet";
  const tutorId = pet.data?.tutorId;

  const compose = () => {
    const care = careLog.data?.pages.flatMap((p) => p.items) ?? [];
    setTitle(`Relatório de ${petName} — ${formatDate(date)}`);
    setContent(buildReportText(petName, date, care, meds.data ?? [], incidents.data ?? []));
  };

  const save = () => {
    if (!tutorId) return;
    create.mutate(
      { tutorId, petId, reservationId, reportDate: date, title, content },
      { onSuccess: () => setContent("") },
    );
  };

  const formError = create.error instanceof ApiError ? create.error.message : null;

  return (
    <div className="space-y-4">
      <div className="space-y-3 rounded-lg border bg-card p-3">
        <div className="flex flex-wrap items-end gap-3">
          <Field label="Dia" htmlFor="report-date">
            <Input id="report-date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </Field>
          <Button type="button" variant="outline" onClick={compose}>
            <FileText /> Montar a partir do diário
          </Button>
        </div>
        {content && (
          <>
            <Field label="Título" htmlFor="report-title">
              <Input id="report-title" value={title} onChange={(e) => setTitle(e.target.value)} />
            </Field>
            <Field label="Mensagem" htmlFor="report-content">
              <Textarea id="report-content" rows={8} value={content} onChange={(e) => setContent(e.target.value)} />
            </Field>
            {formError && <p className="text-sm text-destructive">{formError}</p>}
            <div className="flex justify-end gap-2">
              <Button type="button" variant="ghost" onClick={() => setContent("")}>
                Descartar
              </Button>
              <Button type="button" disabled={create.isPending || !title || !content} onClick={save}>
                {create.isPending ? "Salvando…" : "Salvar relatório"}
              </Button>
            </div>
          </>
        )}
      </div>

      {(reports.data?.length ?? 0) > 0 && (
        <div className="space-y-2">
          {reports.data!.map((r) => (
            <div key={r.id} className="rounded-lg border bg-card p-3 shadow-card">
              <div className="flex flex-wrap items-center gap-2">
                <span className="font-medium">{r.title}</span>
                {r.status === "Sent" ? (
                  <Badge variant="success">Enviado</Badge>
                ) : (
                  <Badge variant="secondary">Rascunho</Badge>
                )}
                <span className="text-xs text-muted-foreground">{formatDate(r.reportDate)}</span>
              </div>
              <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{r.content}</p>
              <div className="mt-2 flex gap-2">
                <Button type="button" variant="outline" size="sm" onClick={() => navigator.clipboard?.writeText(r.content)}>
                  <Copy /> Copiar
                </Button>
                {r.status === "Draft" && (
                  <Button type="button" size="sm" disabled={send.isPending} onClick={() => send.mutate(r.id)}>
                    <Send /> Marcar enviado
                  </Button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
