import { z } from "zod";

export const tutorFormSchema = z
  .object({
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
    // Faturamento (documento em branco = sem faturamento; o form converte para null no submit).
    billingDocument: z.string().max(20).optional().or(z.literal("")),
    billingEmail: z.string().email("E-mail inválido").optional().or(z.literal("")),
    billingAddressLine1: z.string().max(200).optional().or(z.literal("")),
    billingAddressLine2: z.string().max(200).optional().or(z.literal("")),
    billingCity: z.string().max(100).optional().or(z.literal("")),
    billingState: z.string().max(50).optional().or(z.literal("")),
    billingPostalCode: z.string().max(15).optional().or(z.literal("")),
  })
  .superRefine((values, ctx) => {
    const hasDetails =
      !!values.billingEmail ||
      !!values.billingAddressLine1 ||
      !!values.billingAddressLine2 ||
      !!values.billingCity ||
      !!values.billingState ||
      !!values.billingPostalCode;
    if (!values.billingDocument && hasDetails) {
      ctx.addIssue({ code: "custom", path: ["billingDocument"], message: "Informe o CPF/CNPJ" });
    }
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

/** Finalidades de consentimento LGPD (valores batem com ConsentType do backend). */
export const CONSENT_TYPES = ["ImageUse", "Marketing", "DataSharing"] as const;
export const CONSENT_TYPE_LABELS: Record<(typeof CONSENT_TYPES)[number], string> = {
  ImageUse: "Uso de imagem",
  Marketing: "Comunicações de marketing",
  DataSharing: "Compartilhamento com parceiros",
};
export const CONSENT_TYPE_DESCRIPTIONS: Record<(typeof CONSENT_TYPES)[number], string> = {
  ImageUse: "Fotos do pet em redes sociais e materiais de divulgação.",
  Marketing: "Promoções, novidades e campanhas por e-mail ou mensagem.",
  DataSharing: "Compartilhar dados com parceiros (ex.: pet shop, seguradora).",
};

export const FOOD_SOURCES = ["TutorProvided", "HotelProvided"] as const;

/** Rótulos pt-BR da origem da ração (valores batem com o enum FoodSource do backend). */
export const FOOD_SOURCE_LABELS: Record<(typeof FOOD_SOURCES)[number], string> = {
  TutorProvided: "Tutor traz a ração",
  HotelProvided: "Hotel fornece",
};

export const petFormSchema = z
  .object({
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
    // Rotina alimentar (ração em branco = sem rotina; o form converte para null no submit).
    feedingFoodName: z.string().max(200).optional().or(z.literal("")),
    feedingPortionSize: z.string().max(100).optional().or(z.literal("")),
    feedingMealTimes: z.array(z.object({ time: z.string().min(1, "Informe o horário") })),
    feedingRestrictions: z.string().max(1000).optional().or(z.literal("")),
    feedingFoodSource: z.enum(FOOD_SOURCES).or(z.literal("")),
    // Pertences trazidos pelo pet (lista dinâmica).
    belongings: z.array(
      z.object({
        name: z.string().min(1, "Informe o pertence").max(120),
        quantity: z.number({ message: "Quantidade inválida" }).int("Quantidade inválida").min(1, "Mínimo 1"),
        notes: z.string().max(500).optional().or(z.literal("")),
      }),
    ),
  })
  .superRefine((values, ctx) => {
    const hasDetails =
      !!values.feedingPortionSize ||
      values.feedingMealTimes.length > 0 ||
      !!values.feedingRestrictions ||
      values.feedingFoodSource !== "";
    if (!values.feedingFoodName && hasDetails) {
      ctx.addIssue({ code: "custom", path: ["feedingFoodName"], message: "Informe a ração" });
    }
    if (values.feedingFoodName && values.feedingFoodSource === "") {
      ctx.addIssue({ code: "custom", path: ["feedingFoodSource"], message: "Informe a origem da ração" });
    }
  });

export type PetFormInput = z.infer<typeof petFormSchema>;
