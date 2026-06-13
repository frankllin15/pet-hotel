import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Search } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { Spinner } from "@/shared/ui/spinner";
import { Textarea } from "@/shared/ui/textarea";
import { listPets } from "@/features/registry/api";
import { registryKeys } from "@/features/registry/queries";
import { SPECIES_LABELS } from "@/features/registry/schemas";
import type { PackDto } from "../api";
import { useCreatePack, usePack, useUpdatePack } from "../queries";
import { COMPAT_FLAG_LABELS, compatFlags, packFormSchema, type PackFormInput } from "../schemas";

export function PackFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;

  const createMutation = useCreatePack();
  const updateMutation = useUpdatePack(id ?? "");
  const mutation = isEdit ? updateMutation : createMutation;

  const packQuery = usePack(id ?? "");
  const petsQuery = useQuery({
    queryKey: [...registryKeys.pets({}), "selector"],
    queryFn: () => listPets({ limit: 100 }),
  });

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [search, setSearch] = useState("");

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PackFormInput>({
    resolver: zodResolver(packFormSchema),
    defaultValues: { name: "", notes: "" },
  });

  // Pré-preenche nome/observações (RHF reset em efeito — aceito).
  useEffect(() => {
    if (isEdit && packQuery.data) {
      reset({ name: packQuery.data.name, notes: packQuery.data.notes ?? "" });
    }
  }, [isEdit, packQuery.data, reset]);

  // Sincroniza os membros selecionados quando a matilha carrega — padrão "ajustar estado na
  // render" (sem setState em efeito, que o lint barra com "cascading renders").
  const [syncedFrom, setSyncedFrom] = useState<PackDto>();
  if (isEdit && packQuery.data && packQuery.data !== syncedFrom) {
    setSyncedFrom(packQuery.data);
    setSelected(new Set(packQuery.data.members.map((m) => m.petId)));
  }

  const toggle = (petId: string) =>
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(petId)) next.delete(petId);
      else next.add(petId);
      return next;
    });

  const submit = handleSubmit((values) => {
    const body = { name: values.name, notes: values.notes ? values.notes : null, memberPetIds: [...selected] };
    if (isEdit) {
      updateMutation.mutate(
        { id: id!, ...body },
        { onSuccess: () => navigate(`/registry/packs/${id}`, { replace: true }) },
      );
    } else {
      createMutation.mutate(body, { onSuccess: ({ id: newId }) => navigate(`/registry/packs/${newId}`, { replace: true }) });
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;
  const pets = (petsQuery.data?.items ?? []).filter(
    (p) => !search || p.name.toLowerCase().includes(search.toLowerCase()),
  );

  if (isEdit && packQuery.isPending) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  return (
    <FormPage
      title={isEdit ? "Editar matilha" : "Nova matilha"}
      description="Agrupe pets compatíveis para conviverem juntos."
      onSubmit={submit}
      footer={
        <>
          <Button type="button" variant="ghost" onClick={() => navigate(-1)}>
            Cancelar
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Salvar"}
          </Button>
        </>
      }
    >
      <Field label="Nome" htmlFor="name" error={errors.name?.message}>
        <Input id="name" aria-invalid={!!errors.name} {...register("name")} />
      </Field>
      <Field label="Observações" htmlFor="notes" error={errors.notes?.message}>
        <Textarea id="notes" placeholder="Ex.: brincam bem juntos; separar na alimentação." {...register("notes")} />
      </Field>

      <div className="space-y-3 rounded-xl border bg-card p-4 shadow-card">
        <div className="flex items-center justify-between gap-2">
          <h3 className="font-display text-base font-semibold">Membros ({selected.size})</h3>
          <div className="relative w-48">
            <Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
            <Input
              className="pl-8"
              placeholder="Buscar pet…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
        <div className="max-h-72 space-y-1 overflow-y-auto">
          {petsQuery.isPending ? (
            <p className="text-sm text-muted-foreground">Carregando pets…</p>
          ) : pets.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nenhum pet encontrado.</p>
          ) : (
            pets.map((pet) => {
              const flags = compatFlags(pet.sociability, pet.reactivity);
              return (
                <label
                  key={pet.id}
                  className="flex cursor-pointer items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-accent/40"
                >
                  <input
                    type="checkbox"
                    className="size-4 accent-primary"
                    checked={selected.has(pet.id)}
                    onChange={() => toggle(pet.id)}
                  />
                  <span className="flex-1 text-sm font-medium">{pet.name}</span>
                  <span className="text-xs text-muted-foreground">
                    {SPECIES_LABELS[pet.species as keyof typeof SPECIES_LABELS] ?? pet.species}
                  </span>
                  {flags.length > 0 && (
                    <span className="flex items-center gap-1 text-xs text-warning">
                      <AlertTriangle className="size-3.5" />
                      {flags.map((f) => COMPAT_FLAG_LABELS[f]).join(", ")}
                    </span>
                  )}
                </label>
              );
            })
          )}
        </div>
      </div>

      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}
