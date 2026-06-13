import { formatDate, formatTime } from "@/shared/lib/format";
import type { CareLogEntryDto, IncidentDto, MedicationDto } from "@/features/operations/api";
import { CARE_LOG_TYPE_LABELS } from "@/features/operations/schemas";
import { INCIDENT_SEVERITY_LABELS } from "@/features/operations/schemas";

/** Mantém só os itens cujo instante cai no dia informado (yyyy-MM-dd). */
const onDate = (iso: string, date: string) => iso.slice(0, 10) === date;

/**
 * Monta o texto do relatório diário a partir do diário/medicação/incidentes do dia.
 * É um rascunho editável — a equipe revisa antes de enviar.
 */
export function buildReportText(
  petName: string,
  date: string,
  care: CareLogEntryDto[],
  meds: MedicationDto[],
  incidents: IncidentDto[],
): string {
  // Ordenados por horário (ISO ordena cronologicamente) para o resumo ficar em ordem do dia.
  const day = care.filter((e) => onDate(e.occurredAt, date)).sort((a, b) => a.occurredAt.localeCompare(b.occurredAt));
  const dayMeds = meds.filter((m) => onDate(m.administeredAt, date)).sort((a, b) => a.administeredAt.localeCompare(b.administeredAt));
  const dayIncidents = incidents.filter((i) => onDate(i.occurredAt, date)).sort((a, b) => a.occurredAt.localeCompare(b.occurredAt));

  const lines: string[] = [`Olá! Resumo do dia ${formatDate(date)} do(a) ${petName}:`, ""];

  if (day.length > 0) {
    lines.push("Diário:");
    day.forEach((e) =>
      lines.push(`• ${formatTime(e.occurredAt)} — ${CARE_LOG_TYPE_LABELS[e.type] ?? e.type}${e.note ? `: ${e.note}` : ""}`),
    );
    lines.push("");
  }
  if (dayMeds.length > 0) {
    lines.push("Medicação:");
    dayMeds.forEach((m) => lines.push(`• ${formatTime(m.administeredAt)} — ${m.drug} — ${m.dose}`));
    lines.push("");
  }
  if (dayIncidents.length > 0) {
    lines.push("Ocorrências:");
    dayIncidents.forEach((i) =>
      lines.push(`• ${formatTime(i.occurredAt)} — [${INCIDENT_SEVERITY_LABELS[i.severity] ?? i.severity}] ${i.description}`),
    );
    lines.push("");
  }
  if (day.length === 0 && dayMeds.length === 0 && dayIncidents.length === 0) {
    lines.push("Sem registros no diário neste dia.");
  }

  return lines.join("\n").trim();
}
