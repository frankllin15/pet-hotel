import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { useRegisterVaccination } from "../queries";
import { VACCINE_LABELS, VACCINE_TYPES, vaccinationFormSchema, type VaccinationFormInput } from "../schemas";

/** Formulário inline de registro de vacina (arquétipo Form, reusado na ficha). */
export function VaccinationForm({ petId, onDone }: { petId: string; onDone: () => void }) {
  const mutation = useRegisterVaccination(petId);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<VaccinationFormInput>({
    resolver: zodResolver(vaccinationFormSchema),
    defaultValues: { type: "Rabies", appliedOn: "", expiresOn: "" },
  });

  const submit = handleSubmit((values) =>
    mutation.mutate(values, { onSuccess: onDone }),
  );

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
              {mutation.isPending ? "Salvando…" : "Registrar vacina"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
