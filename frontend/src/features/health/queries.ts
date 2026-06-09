import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "@/shared/lib/problem-details";
import { getPetHealth, registerVaccination, type PetHealthDto, type RegisterVaccinationBody } from "./api";

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
