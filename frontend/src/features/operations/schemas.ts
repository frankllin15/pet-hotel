import { z } from "zod";

/** Tipos de ocorrência (valores batem com CareLogEntryType do backend). */
export const CARE_LOG_TYPES = ["Meal", "Bathroom", "Play", "Behavior", "Hygiene", "Note"] as const;

export const CARE_LOG_TYPE_LABELS: Record<string, string> = {
  Meal: "Alimentação",
  Bathroom: "Necessidades",
  Play: "Recreação",
  Behavior: "Comportamento",
  Hygiene: "Higiene",
  Note: "Observação",
};

export const careLogFormSchema = z.object({
  type: z.enum(CARE_LOG_TYPES),
  note: z.string().max(2000).optional().or(z.literal("")),
});
export type CareLogFormInput = z.infer<typeof careLogFormSchema>;

export const medicationFormSchema = z.object({
  drug: z.string().min(1, "Informe o medicamento").max(200),
  dose: z.string().min(1, "Informe a dose").max(100),
});
export type MedicationFormInput = z.infer<typeof medicationFormSchema>;

export const INCIDENT_SEVERITIES = ["Low", "Medium", "High"] as const;
export const INCIDENT_SEVERITY_LABELS: Record<string, string> = {
  Low: "Leve",
  Medium: "Moderado",
  High: "Grave",
};
/** Variante de Badge por gravidade. */
export const INCIDENT_SEVERITY_VARIANTS: Record<string, "secondary" | "warning" | "destructive"> = {
  Low: "secondary",
  Medium: "warning",
  High: "destructive",
};

export const incidentFormSchema = z.object({
  severity: z.enum(INCIDENT_SEVERITIES),
  description: z.string().min(1, "Descreva o incidente").max(2000),
});
export type IncidentFormInput = z.infer<typeof incidentFormSchema>;
