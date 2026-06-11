import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { useRegisterParasiteTreatment } from "../queries";
import {
  PARASITE_TREATMENT_LABELS,
  PARASITE_TREATMENT_TYPES,
  parasiteTreatmentFormSchema,
  type ParasiteTreatmentFormInput,
} from "../schemas";

/** Formulário inline de controle de parasitas (mesmo arquétipo do VaccinationForm). */
export function ParasiteTreatmentForm({ petId, onDone }: { petId: string; onDone: () => void }) {
  const mutation = useRegisterParasiteTreatment(petId);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ParasiteTreatmentFormInput>({
    resolver: zodResolver(parasiteTreatmentFormSchema),
    defaultValues: { type: "FleaTick", productName: "", appliedOn: "", nextDueOn: "" },
  });

  const submit = handleSubmit((values) =>
    mutation.mutate(
      {
        type: values.type,
        productName: values.productName ? values.productName : null,
        appliedOn: values.appliedOn,
        nextDueOn: values.nextDueOn ? values.nextDueOn : null,
      },
      { onSuccess: onDone },
    ),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Card>
      <CardContent className="pt-6">
        <form className="space-y-4" onSubmit={submit}>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Tipo" htmlFor="parasite-type" error={errors.type?.message}>
              <Select id="parasite-type" {...register("type")}>
                {PARASITE_TREATMENT_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {PARASITE_TREATMENT_LABELS[t]}
                  </option>
                ))}
              </Select>
            </Field>
            <Field label="Produto" htmlFor="parasite-product" error={errors.productName?.message}>
              <Input id="parasite-product" placeholder="ex.: Bravecto" {...register("productName")} />
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Aplicado em" htmlFor="parasite-applied" error={errors.appliedOn?.message}>
              <Input id="parasite-applied" type="date" aria-invalid={!!errors.appliedOn} {...register("appliedOn")} />
            </Field>
            <Field label="Próxima dose" htmlFor="parasite-next" error={errors.nextDueOn?.message}>
              <Input id="parasite-next" type="date" aria-invalid={!!errors.nextDueOn} {...register("nextDueOn")} />
            </Field>
          </div>
          {formError && <p className="text-sm text-destructive">{formError}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="ghost" onClick={onDone}>
              Cancelar
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Salvando…" : "Registrar controle"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
