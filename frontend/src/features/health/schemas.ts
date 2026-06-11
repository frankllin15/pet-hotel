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

export const PARASITE_TREATMENT_TYPES = ["FleaTick", "Dewormer"] as const;

/** Rótulos pt-BR dos tipos de controle de parasitas (valores batem com o enum ParasiteTreatmentType do backend). */
export const PARASITE_TREATMENT_LABELS: Record<(typeof PARASITE_TREATMENT_TYPES)[number], string> = {
  FleaTick: "Antipulgas/carrapaticida",
  Dewormer: "Vermífugo",
};

/** Traduz um tipo de controle conhecido; mantém o valor cru se não mapeado. */
export function parasiteTreatmentLabel(type: string): string {
  return (PARASITE_TREATMENT_LABELS as Record<string, string>)[type] ?? type;
}

export const parasiteTreatmentFormSchema = z
  .object({
    type: z.enum(PARASITE_TREATMENT_TYPES),
    productName: z.string().max(200).optional().or(z.literal("")),
    appliedOn: z.string().min(1, "Informe a data de aplicação"),
    nextDueOn: z.string().optional().or(z.literal("")),
  })
  .refine((v) => !v.nextDueOn || v.nextDueOn > v.appliedOn, {
    message: "A próxima dose deve ser posterior à aplicação",
    path: ["nextDueOn"],
  });

export type ParasiteTreatmentFormInput = z.infer<typeof parasiteTreatmentFormSchema>;

export const vetContactFormSchema = z.object({
  name: z.string().min(1, "Informe o nome").max(200),
  phone: z.string().min(1, "Informe o telefone").max(20),
  clinic: z.string().max(200).optional().or(z.literal("")),
});

export type VetContactFormInput = z.infer<typeof vetContactFormSchema>;

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
