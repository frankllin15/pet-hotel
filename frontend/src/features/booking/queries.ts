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
  uploadArrivalPhoto,
  listAccommodations,
  listReservations,
  type ArrivalStateInput,
  type CreateAccommodationBody,
  type CreateReservationBody,
  type UpdateAccommodationBody,
} from "./api";

export const bookingKeys = {
  all: ["booking"] as const,
  accommodations: () => ["booking", "accommodations"] as const,
  reservations: (status?: string) => ["booking", "reservations", { status: status || undefined }] as const,
  reservation: (id: string) => ["booking", "reservation", id] as const,
  occupancy: (from: string, to: string) => ["booking", "occupancy", { from, to }] as const,
};

export function useAccommodations() {
  return useQuery({ queryKey: bookingKeys.accommodations(), queryFn: listAccommodations });
}

export function useReservations(status?: string) {
  return useQuery({
    queryKey: bookingKeys.reservations(status),
    queryFn: () => listReservations(status),
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
