import { z } from "zod";

export const accommodationFormSchema = z.object({
  name: z.string().min(1, "Informe o nome").max(120),
  dailyRate: z.number({ message: "Informe a diária" }).min(0, "A diária não pode ser negativa"),
  capacity: z.number({ message: "Informe a capacidade" }).int().min(1, "A capacidade deve ser de pelo menos 1 pet"),
  active: z.boolean(),
});
export type AccommodationFormInput = z.infer<typeof accommodationFormSchema>;

export const reservationFormSchema = z
  .object({
    petId: z.string().uuid("Selecione um pet"),
    accommodationId: z.string().uuid("Selecione uma acomodação"),
    checkIn: z.string().min(1, "Informe o check-in"),
    checkOut: z.string().min(1, "Informe o check-out"),
  })
  .refine((v) => v.checkOut > v.checkIn, {
    message: "O check-out deve ser posterior ao check-in",
    path: ["checkOut"],
  });
export type ReservationFormInput = z.infer<typeof reservationFormSchema>;

/** Rótulos pt-BR dos status (valores batem com ReservationStatus do backend). */
export const RESERVATION_STATUS = [
  "Requested",
  "Confirmed",
  "CheckedIn",
  "CheckedOut",
  "Cancelled",
] as const;
export const RESERVATION_STATUS_LABELS: Record<string, string> = {
  Requested: "Solicitada",
  Confirmed: "Confirmada",
  CheckedIn: "Em estadia",
  CheckedOut: "Finalizada",
  Cancelled: "Cancelada",
};

/** Rótulos pt-BR dos sinais de compatibilidade (valores batem com PackCompatibilityFlag do backend). */
export const COMPATIBILITY_FLAG_LABELS: Record<string, string> = {
  Reactive: "reatividade alta",
  LowSociability: "baixa sociabilidade",
};

/** Condições de chegada (valores batem com ArrivalCondition do backend). */
export const ARRIVAL_CONDITIONS = ["Healthy", "MinorIssues", "NeedsAttention"] as const;
export const ARRIVAL_CONDITION_LABELS: Record<string, string> = {
  Healthy: "Saudável",
  MinorIssues: "Alterações leves",
  NeedsAttention: "Requer atenção",
};
