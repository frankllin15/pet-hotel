import { usePet } from "@/features/registry/queries";

/**
 * Resolve o nome do pet por Id (módulo Booking referencia pets só por Id).
 * O TanStack Query deduplica as buscas repetidas pela chave.
 */
export function PetName({ petId }: { petId: string }) {
  const { data, isPending } = usePet(petId);
  if (isPending) return <span className="text-muted-foreground">…</span>;
  return <>{data?.name ?? "—"}</>;
}
