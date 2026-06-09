import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  cancelReservation,
  confirmReservation,
  createAccommodation,
  createReservation,
  getOccupancy,
  listAccommodations,
  listReservations,
  type CreateReservationBody,
} from "./api";

export const bookingKeys = {
  all: ["booking"] as const,
  accommodations: () => ["booking", "accommodations"] as const,
  reservations: (status?: string) => ["booking", "reservations", { status: status || undefined }] as const,
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

export function useCancelReservation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => cancelReservation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}
