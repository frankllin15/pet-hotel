import { useEffect } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { Spinner } from "@/shared/ui/spinner";
import { Textarea } from "@/shared/ui/textarea";
import { getPet, listTutors, type Species } from "../api";
import { registryKeys, useRegisterPet, useUpdatePet } from "../queries";
import {
  petFormSchema,
  BEHAVIOR_LEVELS,
  BEHAVIOR_LEVEL_LABELS,
  BEHAVIOR_TRAITS,
  FOOD_SOURCES,
  FOOD_SOURCE_LABELS,
  PET_SIZES,
  PET_SIZE_LABELS,
  SEXES,
  SEX_LABELS,
  SPECIES,
  SPECIES_LABELS,
  type PetFormInput,
} from "../schemas";

export function PetFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const [searchParams] = useSearchParams();
  const presetTutorId = searchParams.get("tutorId") ?? "";

  const registerMutation = useRegisterPet();
  const updateMutation = useUpdatePet(id ?? "");
  const mutation = isEdit ? updateMutation : registerMutation;

  // Em edição, carrega o pet para pré-preencher; o seletor de tutor não aparece.
  const petQuery = useQuery({ queryKey: registryKeys.pet(id ?? ""), queryFn: () => getPet(id!), enabled: isEdit });

  // Sem tutor pré-selecionado (e criação), carrega a primeira página para o seletor.
  const tutorsQuery = useQuery({
    queryKey: [...registryKeys.tutors(), "selector"],
    queryFn: () => listTutors({ limit: 100 }),
    enabled: !isEdit && presetTutorId === "",
  });

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PetFormInput>({
    resolver: zodResolver(petFormSchema),
    defaultValues: {
      tutorId: presetTutorId,
      species: "Dog",
      breed: "",
      birthDate: "",
      size: "",
      sex: "",
      neutered: "",
      microchipCode: "",
      notes: "",
      sociability: "",
      reactivity: "",
      fear: "",
      destructiveness: "",
      behaviorNotes: "",
      feedingFoodName: "",
      feedingPortionSize: "",
      feedingMealTimes: [],
      feedingRestrictions: "",
      feedingFoodSource: "",
    },
  });

  const mealTimes = useFieldArray({ control, name: "feedingMealTimes" });

  // Pré-preenche o formulário quando o pet é carregado (edição).
  useEffect(() => {
    const pet = petQuery.data;
    if (!pet) return;
    reset({
      tutorId: pet.tutorId,
      name: pet.name,
      species: pet.species as Species,
      breed: pet.breed ?? "",
      birthDate: pet.birthDate ?? "",
      size: (pet.size ?? "") as PetFormInput["size"],
      sex: (pet.sex ?? "") as PetFormInput["sex"],
      neutered: pet.neutered === null || pet.neutered === undefined ? "" : pet.neutered ? "yes" : "no",
      microchipCode: pet.microchipCode ?? "",
      notes: pet.notes ?? "",
      sociability: (pet.sociability ?? "") as PetFormInput["sociability"],
      reactivity: (pet.reactivity ?? "") as PetFormInput["reactivity"],
      fear: (pet.fear ?? "") as PetFormInput["fear"],
      destructiveness: (pet.destructiveness ?? "") as PetFormInput["destructiveness"],
      behaviorNotes: pet.behaviorNotes ?? "",
      feedingFoodName: pet.feedingRoutine?.foodName ?? "",
      feedingPortionSize: pet.feedingRoutine?.portionSize ?? "",
      // O backend serializa TimeOnly como "HH:mm:ss"; o input type="time" usa "HH:mm".
      feedingMealTimes: pet.feedingRoutine?.mealTimes.map((t) => ({ time: t.slice(0, 5) })) ?? [],
      feedingRestrictions: pet.feedingRoutine?.restrictions ?? "",
      feedingFoodSource: (pet.feedingRoutine?.foodSource ?? "") as PetFormInput["feedingFoodSource"],
    });
  }, [petQuery.data, reset]);

  const submit = handleSubmit((values) => {
    const common = {
      name: values.name,
      species: values.species,
      breed: values.breed ? values.breed : null,
      birthDate: values.birthDate ? values.birthDate : null,
      size: values.size ? values.size : null,
      sex: values.sex ? values.sex : null,
      neutered: values.neutered === "" ? null : values.neutered === "yes",
      microchipCode: values.microchipCode ? values.microchipCode : null,
      notes: values.notes ? values.notes : null,
      feedingRoutine:
        values.feedingFoodName && values.feedingFoodSource !== ""
          ? {
              foodName: values.feedingFoodName,
              portionSize: values.feedingPortionSize ? values.feedingPortionSize : null,
              mealTimes: values.feedingMealTimes.map((m) => m.time),
              restrictions: values.feedingRestrictions ? values.feedingRestrictions : null,
              foodSource: values.feedingFoodSource,
            }
          : null,
    };

    if (isEdit) {
      updateMutation.mutate(
        {
          id: id!,
          ...common,
          sociability: values.sociability ? values.sociability : null,
          reactivity: values.reactivity ? values.reactivity : null,
          fear: values.fear ? values.fear : null,
          destructiveness: values.destructiveness ? values.destructiveness : null,
          behaviorNotes: values.behaviorNotes ? values.behaviorNotes : null,
        },
        { onSuccess: () => navigate(`/registry/pets/${id}`, { replace: true }) },
      );
    } else {
      registerMutation.mutate(
        { tutorId: values.tutorId, ...common },
        { onSuccess: ({ id: newId }) => navigate(`/registry/pets/${newId}`, { replace: true }) },
      );
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  if (isEdit && petQuery.isPending) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  return (
    <FormPage
      title={isEdit ? "Editar pet" : "Novo pet"}
      description={isEdit ? "Atualize os dados do pet." : "Cadastre um pet para um tutor existente."}
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
      {!isEdit && presetTutorId === "" ? (
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

      <Field label="Porte" htmlFor="size" error={errors.size?.message}>
        <Select id="size" {...register("size")}>
          <option value="">Não informado</option>
          {PET_SIZES.map((s) => (
            <option key={s} value={s}>
              {PET_SIZE_LABELS[s]}
            </option>
          ))}
        </Select>
      </Field>

      <Field label="Sexo" htmlFor="sex" error={errors.sex?.message}>
        <Select id="sex" {...register("sex")}>
          <option value="">Não informado</option>
          {SEXES.map((s) => (
            <option key={s} value={s}>
              {SEX_LABELS[s]}
            </option>
          ))}
        </Select>
      </Field>

      <Field label="Castrado" htmlFor="neutered" error={errors.neutered?.message}>
        <Select id="neutered" {...register("neutered")}>
          <option value="">Não informado</option>
          <option value="yes">Sim</option>
          <option value="no">Não</option>
        </Select>
      </Field>

      <Field label="Microchip" htmlFor="microchipCode" error={errors.microchipCode?.message}>
        <Input id="microchipCode" {...register("microchipCode")} />
      </Field>

      <Field label="Observações" htmlFor="notes" error={errors.notes?.message}>
        <Textarea id="notes" {...register("notes")} />
      </Field>

      <div className="space-y-3 rounded-lg border p-4">
        <div>
          <h3 className="text-sm font-semibold">Rotina alimentar</h3>
          <p className="text-xs text-muted-foreground">
            Ração, quantidade, horários e restrições. Deixe a ração em branco se não houver rotina.
          </p>
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Ração" htmlFor="feedingFoodName" error={errors.feedingFoodName?.message}>
            <Input
              id="feedingFoodName"
              placeholder="ex.: Golden Filhote"
              aria-invalid={!!errors.feedingFoodName}
              {...register("feedingFoodName")}
            />
          </Field>
          <Field label="Quantidade por refeição" htmlFor="feedingPortionSize" error={errors.feedingPortionSize?.message}>
            <Input id="feedingPortionSize" placeholder="ex.: 100 g" {...register("feedingPortionSize")} />
          </Field>
        </div>
        <Field label="Origem da ração" htmlFor="feedingFoodSource" error={errors.feedingFoodSource?.message}>
          <Select id="feedingFoodSource" aria-invalid={!!errors.feedingFoodSource} {...register("feedingFoodSource")}>
            <option value="">Não informado</option>
            {FOOD_SOURCES.map((source) => (
              <option key={source} value={source}>
                {FOOD_SOURCE_LABELS[source]}
              </option>
            ))}
          </Select>
        </Field>
        <div className="space-y-2">
          <div className="flex items-center justify-between gap-2">
            <span className="text-sm font-medium">Horários das refeições</span>
            <Button type="button" variant="outline" size="sm" onClick={() => mealTimes.append({ time: "" })}>
              <Plus /> Adicionar horário
            </Button>
          </div>
          {mealTimes.fields.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nenhum horário.</p>
          ) : (
            mealTimes.fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <Field
                  label={`Refeição ${index + 1}`}
                  htmlFor={`meal-time-${index}`}
                  error={errors.feedingMealTimes?.[index]?.time?.message}
                >
                  <Input id={`meal-time-${index}`} type="time" {...register(`feedingMealTimes.${index}.time`)} />
                </Field>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  aria-label="Remover horário"
                  onClick={() => mealTimes.remove(index)}
                >
                  <Trash2 />
                </Button>
              </div>
            ))
          )}
        </div>
        <Field label="Restrições alimentares" htmlFor="feedingRestrictions" error={errors.feedingRestrictions?.message}>
          <Textarea
            id="feedingRestrictions"
            placeholder="ex.: alergia a frango"
            {...register("feedingRestrictions")}
          />
        </Field>
      </div>

      {isEdit && (
        <div className="space-y-3 rounded-lg border p-4">
          <div>
            <h3 className="text-sm font-semibold">Avaliação comportamental</h3>
            <p className="text-xs text-muted-foreground">
              Observada durante a estadia — base para a compatibilidade em matilhas.
            </p>
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            {BEHAVIOR_TRAITS.map((trait) => (
              <Field key={trait.key} label={trait.label} htmlFor={trait.key}>
                <Select id={trait.key} {...register(trait.key)}>
                  <option value="">Não informado</option>
                  {BEHAVIOR_LEVELS.map((level) => (
                    <option key={level} value={level}>
                      {BEHAVIOR_LEVEL_LABELS[level]}
                    </option>
                  ))}
                </Select>
              </Field>
            ))}
          </div>
          <Field label="Notas comportamentais" htmlFor="behaviorNotes" error={errors.behaviorNotes?.message}>
            <Textarea id="behaviorNotes" {...register("behaviorNotes")} />
          </Field>
        </div>
      )}

      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}
