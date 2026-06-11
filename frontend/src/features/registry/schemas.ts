import { z } from "zod";

export const tutorFormSchema = z.object({
  fullName: z.string().min(1, "Informe o nome").max(200),
  email: z.string().min(1, "Informe o e-mail").email("E-mail inválido"),
  phone: z.string().min(1, "Informe o telefone").max(20),
  emergencyContacts: z.array(
    z.object({
      name: z.string().min(1, "Informe o nome").max(200),
      phone: z.string().min(1, "Informe o telefone").max(20),
      relationship: z.string().max(100).optional().or(z.literal("")),
    }),
  ),
  authorizedPickups: z.array(
    z.object({
      name: z.string().min(1, "Informe o nome").max(200),
      document: z.string().max(50).optional().or(z.literal("")),
    }),
  ),
});

export type TutorFormInput = z.infer<typeof tutorFormSchema>;

export const SPECIES = ["Dog", "Cat", "Other"] as const;

/** Rótulos pt-BR das espécies (valores batem com o enum Species do backend). */
export const SPECIES_LABELS: Record<(typeof SPECIES)[number], string> = {
  Dog: "Cão",
  Cat: "Gato",
  Other: "Outro",
};

export const PET_SIZES = ["Small", "Medium", "Large", "Giant"] as const;

/** Rótulos pt-BR de porte (valores batem com o enum PetSize do backend). */
export const PET_SIZE_LABELS: Record<(typeof PET_SIZES)[number], string> = {
  Small: "Pequeno",
  Medium: "Médio",
  Large: "Grande",
  Giant: "Gigante",
};

export const SEXES = ["Male", "Female"] as const;

/** Rótulos pt-BR de sexo (valores batem com o enum Sex do backend). */
export const SEX_LABELS: Record<(typeof SEXES)[number], string> = {
  Male: "Macho",
  Female: "Fêmea",
};

export const BEHAVIOR_LEVELS = ["Low", "Medium", "High"] as const;

/** Rótulos pt-BR de nível comportamental (valores batem com o enum BehaviorLevel do backend). */
export const BEHAVIOR_LEVEL_LABELS: Record<(typeof BEHAVIOR_LEVELS)[number], string> = {
  Low: "Baixa",
  Medium: "Média",
  High: "Alta",
};

/** Traços avaliados + rótulo de exibição. */
export const BEHAVIOR_TRAITS = [
  { key: "sociability", label: "Sociabilidade" },
  { key: "reactivity", label: "Reatividade" },
  { key: "fear", label: "Medo" },
  { key: "destructiveness", label: "Destrutividade" },
] as const;

export const petFormSchema = z.object({
  tutorId: z.string().uuid("Selecione um tutor"),
  name: z.string().min(1, "Informe o nome").max(120),
  species: z.enum(SPECIES),
  breed: z.string().max(120).optional().or(z.literal("")),
  birthDate: z.string().optional().or(z.literal("")),
  // "" = não informado; o form converte para null no submit.
  size: z.enum(PET_SIZES).or(z.literal("")),
  sex: z.enum(SEXES).or(z.literal("")),
  neutered: z.enum(["", "yes", "no"]),
  microchipCode: z.string().max(50).optional().or(z.literal("")),
  notes: z.string().max(1000).optional().or(z.literal("")),
  // Avaliação comportamental ("" = não informado).
  sociability: z.enum(BEHAVIOR_LEVELS).or(z.literal("")),
  reactivity: z.enum(BEHAVIOR_LEVELS).or(z.literal("")),
  fear: z.enum(BEHAVIOR_LEVELS).or(z.literal("")),
  destructiveness: z.enum(BEHAVIOR_LEVELS).or(z.literal("")),
  behaviorNotes: z.string().max(2000).optional().or(z.literal("")),
});

export type PetFormInput = z.infer<typeof petFormSchema>;
