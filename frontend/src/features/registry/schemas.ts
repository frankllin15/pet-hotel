import { z } from "zod";

export const tutorFormSchema = z.object({
  fullName: z.string().min(1, "Informe o nome").max(200),
  email: z.string().min(1, "Informe o e-mail").email("E-mail inválido"),
  phone: z.string().min(1, "Informe o telefone").max(20),
});

export type TutorFormInput = z.infer<typeof tutorFormSchema>;

export const SPECIES = ["Dog", "Cat", "Other"] as const;

/** Rótulos pt-BR das espécies (valores batem com o enum Species do backend). */
export const SPECIES_LABELS: Record<(typeof SPECIES)[number], string> = {
  Dog: "Cão",
  Cat: "Gato",
  Other: "Outro",
};

export const petFormSchema = z.object({
  tutorId: z.string().uuid("Selecione um tutor"),
  name: z.string().min(1, "Informe o nome").max(120),
  species: z.enum(SPECIES),
  breed: z.string().max(120).optional().or(z.literal("")),
  birthDate: z.string().optional().or(z.literal("")),
  notes: z.string().max(1000).optional().or(z.literal("")),
});

export type PetFormInput = z.infer<typeof petFormSchema>;
