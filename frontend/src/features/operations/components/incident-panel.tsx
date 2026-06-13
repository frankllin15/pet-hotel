import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Plus } from "lucide-react";
import { formatDateTime } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { Select } from "@/shared/ui/select";
import { Textarea } from "@/shared/ui/textarea";
import { UserName } from "@/features/users/components/user-name";
import { useReportIncident, useStayIncidents } from "../queries";
import {
  INCIDENT_SEVERITIES,
  INCIDENT_SEVERITY_LABELS,
  INCIDENT_SEVERITY_VARIANTS,
  incidentFormSchema,
  type IncidentFormInput,
} from "../schemas";

/** Registro auditável de incidentes na estadia (gravidade + descrição). */
export function IncidentPanel({ reservationId, canManage }: { reservationId: string; canManage: boolean }) {
  const query = useStayIncidents(reservationId);
  const report = useReportIncident(reservationId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<IncidentFormInput>({
    resolver: zodResolver(incidentFormSchema),
    defaultValues: { severity: "Low", description: "" },
  });

  const submit = handleSubmit((values) => report.mutate(values, { onSuccess: () => reset({ severity: values.severity, description: "" }) }));
  const formError = report.error instanceof ApiError ? report.error.message : null;

  return (
    <div className="space-y-4">
      {canManage && (
        <form className="space-y-3" onSubmit={submit}>
          <div className="w-40">
            <Field label="Gravidade" htmlFor="incident-severity" error={errors.severity?.message}>
              <Select id="incident-severity" {...register("severity")}>
                {INCIDENT_SEVERITIES.map((s) => (
                  <option key={s} value={s}>
                    {INCIDENT_SEVERITY_LABELS[s]}
                  </option>
                ))}
              </Select>
            </Field>
          </div>
          <Field label="Descrição" htmlFor="incident-description" error={errors.description?.message}>
            <Textarea id="incident-description" rows={2} aria-invalid={!!errors.description} {...register("description")} />
          </Field>
          <div className="flex justify-end">
            <Button type="submit" disabled={report.isPending}>
              <Plus /> {report.isPending ? "Registrando…" : "Registrar incidente"}
            </Button>
          </div>
        </form>
      )}
      {formError && <p className="text-sm text-destructive">{formError}</p>}

      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={<p className="rounded-lg border border-dashed py-6 text-center text-sm text-muted-foreground">Nenhum incidente registrado.</p>}
      >
        {(incidents) => (
          <div className="space-y-3">
            {incidents.map((i) => (
              <div key={i.id} className="rounded-lg border bg-card p-3 shadow-card">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant={INCIDENT_SEVERITY_VARIANTS[i.severity] ?? "secondary"}>
                    {INCIDENT_SEVERITY_LABELS[i.severity] ?? i.severity}
                  </Badge>
                  <span className="text-xs text-muted-foreground">{formatDateTime(i.occurredAt)}</span>
                  {i.reportedBy && (
                    <span className="text-xs text-muted-foreground">
                      · <UserName userId={i.reportedBy} />
                    </span>
                  )}
                </div>
                <p className="mt-1 text-sm">{i.description}</p>
              </div>
            ))}
          </div>
        )}
      </AsyncBoundary>
    </div>
  );
}
