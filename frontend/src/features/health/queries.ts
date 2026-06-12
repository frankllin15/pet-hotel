import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "@/shared/lib/problem-details";
import {
  deleteVaccinationPhoto,
  getPetHealth,
  registerParasiteTreatment,
  registerVaccination,
  setVetContact,
  uploadVaccinationPhoto,
  type PetHealthDto,
  type RegisterParasiteTreatmentBody,
  type RegisterVaccinationBody,
  type SetVetContactBody,
} from "./api";

export const healthKeys = {
  pet: (petId: string) => ["health", "pet", petId] as const,
};

/**
 * Ficha de saúde do pet. O backend devolve 404 quando ainda não há ficha
 * (nenhuma vacina) — tratamos como `null` (sem registro), não como erro.
 */
export function usePetHealth(petId: string) {
  return useQuery<PetHealthDto | null>({
    queryKey: healthKeys.pet(petId),
    queryFn: async () => {
      try {
        return await getPetHealth(petId);
      } catch (error) {
        if (error instanceof ApiError && error.kind === "notfound") {
          return null;
        }
        throw error;
      }
    },
  });
}

export function useRegisterVaccination(petId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: RegisterVaccinationBody) => registerVaccination(petId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: healthKeys.pet(petId) });
    },
  });
}

export function useRegisterParasiteTreatment(petId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: RegisterParasiteTreatmentBody) => registerParasiteTreatment(petId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: healthKeys.pet(petId) });
    },
  });
}

/** Upload e remoção da foto da carteira por vacinação, invalidando a ficha de saúde. */
export function useVaccinationPhoto(petId: string) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: healthKeys.pet(petId) });

  const upload = useMutation({
    mutationFn: ({ vaccinationId, file }: { vaccinationId: string; file: File }) =>
      uploadVaccinationPhoto(petId, vaccinationId, file),
    onSuccess: invalidate,
  });
  const remove = useMutation({
    mutationFn: (vaccinationId: string) => deleteVaccinationPhoto(petId, vaccinationId),
    onSuccess: invalidate,
  });

  return { upload, remove };
}

export function useSetVetContact(petId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: SetVetContactBody) => setVetContact(petId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: healthKeys.pet(petId) });
    },
  });
}
