import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  cancelReservation,
  checkInReservation,
  checkOutReservation,
  confirmReservation,
  createAccommodation,
  createReservation,
  getOccupancy,
  getReservation,
  listAccommodations,
  listReservations,
  type ArrivalStateInput,
  type CreateReservationBody,
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
    mutationFn: (name: string) => createAccommodation(name),
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

export function useCancelReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => cancelReservation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}
