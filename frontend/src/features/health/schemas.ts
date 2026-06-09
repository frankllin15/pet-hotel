import { z } from "zod";

export const VACCINE_TYPES = [
  "Rabies",
  "Distemper",
  "Parvovirus",
  "Bordetella",
  "FelineLeukemia",
  "Other",
] as const;

/** Rótulos pt-BR dos tipos de vacina (valores batem com o enum VaccineType do backend). */
export const VACCINE_LABELS: Record<(typeof VACCINE_TYPES)[number], string> = {
  Rabies: "Raiva",
  Distemper: "Cinomose",
  Parvovirus: "Parvovirose",
  Bordetella: "Tosse dos canis (Bordetella)",
  FelineLeukemia: "Leucemia felina",
  Other: "Outra",
};

/** Traduz um tipo de vacina conhecido; mantém o valor cru se não mapeado. */
export function vaccineLabel(type: string): string {
  return (VACCINE_LABELS as Record<string, string>)[type] ?? type;
}

export const vaccinationFormSchema = z
  .object({
    type: z.enum(VACCINE_TYPES),
    appliedOn: z.string().min(1, "Informe a data de aplicação"),
    expiresOn: z.string().min(1, "Informe a validade"),
  })
  .refine((v) => v.expiresOn >= v.appliedOn, {
    message: "A validade deve ser igual ou posterior à aplicação",
    path: ["expiresOn"],
  });

export type VaccinationFormInput = z.infer<typeof vaccinationFormSchema>;
