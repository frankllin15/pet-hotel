import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  cancelReservation,
  checkInReservation,
  checkOutReservation,
  confirmReservation,
  createAccommodation,
  updateAccommodation,
  createReservation,
  deleteArrivalPhoto,
  getOccupancy,
  getReservation,
  getSharingCompatibility,
  uploadArrivalPhoto,
  listAccommodations,
  listReservations,
  type ArrivalStateInput,
  type CreateAccommodationBody,
  type CreateReservationBody,
  type ReservationFilters,
  type SharingCompatibilityParams,
  type UpdateAccommodationBody,
} from "./api";

export const bookingKeys = {
  all: ["booking"] as const,
  accommodations: () => ["booking", "accommodations"] as const,
  reservations: (filters: ReservationFilters) => ["booking", "reservations", filters] as const,
  reservation: (id: string) => ["booking", "reservation", id] as const,
  occupancy: (from: string, to: string) => ["booking", "occupancy", { from, to }] as const,
  compatibility: (params: SharingCompatibilityParams) => ["booking", "compatibility", params] as const,
};

export function useAccommodations() {
  return useQuery({ queryKey: bookingKeys.accommodations(), queryFn: listAccommodations });
}

export function useReservations(filters: ReservationFilters = {}) {
  return useQuery({
    queryKey: bookingKeys.reservations(filters),
    queryFn: () => listReservations(filters),
    placeholderData: (prev) => prev, // mantém a página atual visível ao trocar de página/filtro
  });
}

export function useReservation(id: string) {
  return useQuery({
    queryKey: bookingKeys.reservation(id),
    queryFn: () => getReservation(id),
    enabled: !!id,
  });
}

export function useOccupancy(from: string, to: string) {
  return useQuery({
    queryKey: bookingKeys.occupancy(from, to),
    queryFn: () => getOccupancy(from, to),
  });
}

/**
 * Alerta de compatibilidade ao compartilhar a acomodação. Só consulta quando todos os campos
 * estão preenchidos e o período é válido; não bloqueia a reserva (é só aviso).
 */
export function useSharingCompatibility(params: Partial<SharingCompatibilityParams>) {
  const ready =
    !!params.accommodationId &&
    !!params.petId &&
    !!params.checkIn &&
    !!params.checkOut &&
    params.checkOut > params.checkIn;

  return useQuery({
    queryKey: bookingKeys.compatibility(params as SharingCompatibilityParams),
    queryFn: () => getSharingCompatibility(params as SharingCompatibilityParams),
    enabled: ready,
  });
}

export function useCreateAccommodation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateAccommodationBody) => createAccommodation(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.accommodations() }),
  });
}

export function useUpdateAccommodation(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateAccommodationBody) => updateAccommodation(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.accommodations() }),
  });
}

export function useCreateReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateReservationBody) => createReservation(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

export function useConfirmReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => confirmReservation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

export function useCheckInReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (vars: { id: string; arrivalState?: ArrivalStateInput }) =>
      checkInReservation(vars.id, vars.arrivalState),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

export function useCheckOutReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => checkOutReservation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

/** Upload e remoção de fotos de chegada, invalidando a ficha da reserva após cada operação. */
export function useArrivalPhotos(reservationId: string) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: bookingKeys.reservation(reservationId) });

  const upload = useMutation({ mutationFn: (file: File) => uploadArrivalPhoto(reservationId, file), onSuccess: invalidate });
  const remove = useMutation({ mutationFn: (key: string) => deleteArrivalPhoto(reservationId, key), onSuccess: invalidate });

  return { upload, remove };
}

export function useCancelReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => cancelReservation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}
