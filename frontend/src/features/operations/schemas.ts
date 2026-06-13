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
