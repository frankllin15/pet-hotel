import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createPack,
  deletePack,
  getPack,
  listPacks,
  updatePack,
  type CreatePackBody,
  type UpdatePackBody,
} from "./api";

export const packKeys = {
  all: ["packs"] as const,
  list: () => ["packs", "list"] as const,
  detail: (id: string) => ["packs", "detail", id] as const,
};

export function usePacks() {
  return useQuery({ queryKey: packKeys.list(), queryFn: listPacks });
}

export function usePack(id: string) {
  return useQuery({ queryKey: packKeys.detail(id), queryFn: () => getPack(id), enabled: !!id });
}

export function useCreatePack() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreatePackBody) => createPack(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: packKeys.all }),
  });
}

export function useUpdatePack(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdatePackBody) => updatePack(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: packKeys.all }),
  });
}

export function useDeletePack() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deletePack(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: packKeys.all }),
  });
}
