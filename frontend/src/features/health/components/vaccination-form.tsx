import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import type { VaccinationDto } from "../api";
import { useRegisterVaccination, useUpdateVaccination } from "../queries";
import {
  VACCINE_LABELS,
  VACCINE_TYPES,
  vaccinationFormSchema,
  type VaccinationFormInput,
} from "../schemas";

/** Formulário inline de vacina (registro ou edição quando `vaccination` é informada). */
export function VaccinationForm({
  petId,
  vaccination,
  onDone,
}: {
  petId: string;
  vaccination?: VaccinationDto;
  onDone: () => void;
}) {
  const isEdit = vaccination !== undefined;
  const registerMutation = useRegisterVaccination(petId);
  const updateMutation = useUpdateVaccination(petId);
  const mutation = isEdit ? updateMutation : registerMutation;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<VaccinationFormInput>({
    resolver: zodResolver(vaccinationFormSchema),
    defaultValues: vaccination
      ? {
          type: vaccination.type as VaccinationFormInput["type"],
          appliedOn: vaccination.appliedOn,
          expiresOn: vaccination.expiresOn,
        }
      : { type: "Rabies", appliedOn: "", expiresOn: "" },
  });

  const submit = handleSubmit((values) => {
    if (isEdit) {
      updateMutation.mutate({ vaccinationId: vaccination.id, body: values }, { onSuccess: onDone });
    } else {
      registerMutation.mutate(values, { onSuccess: onDone });
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Card>
      <CardContent className="pt-6">
        <form className="space-y-4" onSubmit={submit}>
          <Field label="Vacina" htmlFor="type" error={errors.type?.message}>
            <Select id="type" {...register("type")}>
              {VACCINE_TYPES.map((t) => (
                <option key={t} value={t}>
                  {VACCINE_LABELS[t]}
                </option>
              ))}
            </Select>
          </Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Aplicada em" htmlFor="appliedOn" error={errors.appliedOn?.message}>
              <Input id="appliedOn" type="date" aria-invalid={!!errors.appliedOn} {...register("appliedOn")} />
            </Field>
            <Field label="Validade" htmlFor="expiresOn" error={errors.expiresOn?.message}>
              <Input id="expiresOn" type="date" aria-invalid={!!errors.expiresOn} {...register("expiresOn")} />
            </Field>
          </div>
          {formError && <p className="text-sm text-destructive">{formError}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="ghost" onClick={onDone}>
              Cancelar
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Salvando…" : isEdit ? "Salvar alterações" : "Registrar vacina"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
