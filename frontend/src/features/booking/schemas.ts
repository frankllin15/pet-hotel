import { z } from "zod";

export const accommodationFormSchema = z.object({
  name: z.string().min(1, "Informe o nome").max(120),
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
