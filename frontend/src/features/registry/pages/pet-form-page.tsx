import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { Textarea } from "@/shared/ui/textarea";
import { listTutors } from "../api";
import { registryKeys, useRegisterPet } from "../queries";
import { petFormSchema, SPECIES, SPECIES_LABELS, type PetFormInput } from "../schemas";

export function PetFormPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const presetTutorId = searchParams.get("tutorId") ?? "";
  const mutation = useRegisterPet();

  // Quando não há tutor pré-selecionado, carrega a primeira página para o seletor.
  const tutorsQuery = useQuery({
    queryKey: [...registryKeys.tutors(), "selector"],
    queryFn: () => listTutors({ limit: 100 }),
    enabled: presetTutorId === "",
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PetFormInput>({
    resolver: zodResolver(petFormSchema),
    defaultValues: { tutorId: presetTutorId, species: "Dog", breed: "", birthDate: "", notes: "" },
  });

  const submit = handleSubmit((values) =>
    mutation.mutate(
      {
        tutorId: values.tutorId,
        name: values.name,
        species: values.species,
        breed: values.breed ? values.breed : null,
        birthDate: values.birthDate ? values.birthDate : null,
        notes: values.notes ? values.notes : null,
      },
      { onSuccess: ({ id }) => navigate(`/registry/pets/${id}`, { replace: true }) },
    ),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <FormPage
      title="Novo pet"
      description="Cadastre um pet para um tutor existente."
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
      {presetTutorId === "" ? (
        <Field label="Tutor" htmlFor="tutorId" error={errors.tutorId?.message}>
          <Select id="tutorId" aria-invalid={!!errors.tutorId} defaultValue="" {...register("tutorId")}>
            <option value="" disabled>
              {tutorsQuery.isPending ? "Carregando tutores…" : "Selecione um tutor"}
            </option>
            {tutorsQuery.data?.items.map((tutor) => (
              <option key={tutor.id} value={tutor.id}>
                {tutor.fullName}
              </option>
            ))}
          </Select>
        </Field>
      ) : (
        <input type="hidden" {...register("tutorId")} />
      )}

      <Field label="Nome" htmlFor="name" error={errors.name?.message}>
        <Input id="name" aria-invalid={!!errors.name} {...register("name")} />
      </Field>

      <Field label="Espécie" htmlFor="species" error={errors.species?.message}>
        <Select id="species" {...register("species")}>
          {SPECIES.map((s) => (
            <option key={s} value={s}>
              {SPECIES_LABELS[s]}
            </option>
          ))}
        </Select>
      </Field>

      <Field label="Raça" htmlFor="breed" error={errors.breed?.message}>
        <Input id="breed" {...register("breed")} />
      </Field>

      <Field label="Nascimento" htmlFor="birthDate" error={errors.birthDate?.message}>
        <Input id="birthDate" type="date" {...register("birthDate")} />
      </Field>

      <Field label="Observações" htmlFor="notes" error={errors.notes?.message}>
        <Textarea id="notes" {...register("notes")} />
      </Field>

      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}
