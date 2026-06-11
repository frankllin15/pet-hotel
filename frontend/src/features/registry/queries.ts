import {
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import {
  getPet,
  getTutor,
  listPets,
  listTutors,
  registerPet,
  registerTutor,
  updatePet,
  updateTutor,
  type ListPetsParams,
  type RegisterPetBody,
  type RegisterTutorBody,
  type UpdatePetBody,
  type UpdateTutorBody,
} from "./api";

const PAGE_SIZE = 20;

/** Chaves de query padronizadas por feature (docs/08 §Estado). */
export const registryKeys = {
  all: ["registry"] as const,
  tutors: (search?: string) => ["registry", "tutors", { search: search || undefined }] as const,
  tutor: (id: string) => ["registry", "tutor", id] as const,
  pets: (params: Omit<ListPetsParams, "cursor" | "limit">) =>
    ["registry", "pets", { search: params.search || undefined, tutorId: params.tutorId }] as const,
  pet: (id: string) => ["registry", "pet", id] as const,
};

export function useTutors(search?: string) {
  return useInfiniteQuery({
    queryKey: registryKeys.tutors(search),
    queryFn: ({ pageParam }) => listTutors({ search, cursor: pageParam, limit: PAGE_SIZE }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });
}

export function useTutor(id: string) {
  return useQuery({
    queryKey: registryKeys.tutor(id),
    queryFn: () => getTutor(id),
  });
}

export function usePets(params: Omit<ListPetsParams, "cursor" | "limit">) {
  return useInfiniteQuery({
    queryKey: registryKeys.pets(params),
    queryFn: ({ pageParam }) => listPets({ ...params, cursor: pageParam, limit: PAGE_SIZE }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });
}

export function usePet(id: string) {
  return useQuery({
    queryKey: registryKeys.pet(id),
    queryFn: () => getPet(id),
  });
}

export function useRegisterTutor() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: RegisterTutorBody) => registerTutor(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: registryKeys.all });
    },
  });
}

export function useRegisterPet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: RegisterPetBody) => registerPet(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: registryKeys.all });
    },
  });
}

export function useUpdateTutor(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateTutorBody) => updateTutor(id, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: registryKeys.all });
    },
  });
}

export function useUpdatePet(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdatePetBody) => updatePet(id, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: registryKeys.all });
    },
  });
}
