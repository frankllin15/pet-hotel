import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Plus } from "lucide-react";
import { formatDateTime } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { UserName } from "@/features/users/components/user-name";
import { useRecordMedication, useStayMedications } from "../queries";
import { medicationFormSchema, type MedicationFormInput } from "../schemas";

/** Log de administração de medicamento na estadia (quem/quando/dose). */
export function MedicationPanel({ reservationId, canManage }: { reservationId: string; canManage: boolean }) {
  const query = useStayMedications(reservationId);
  const record = useRecordMedication(reservationId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<MedicationFormInput>({ resolver: zodResolver(medicationFormSchema), defaultValues: { drug: "", dose: "" } });

  const submit = handleSubmit((values) => record.mutate(values, { onSuccess: () => reset({ drug: "", dose: "" }) }));
  const formError = record.error instanceof ApiError ? record.error.message : null;

  return (
    <div className="space-y-4">
      {canManage && (
        <form className="flex flex-wrap items-end gap-3" onSubmit={submit}>
          <div className="min-w-40 flex-1">
            <Field label="Medicamento" htmlFor="med-drug" error={errors.drug?.message}>
              <Input id="med-drug" placeholder="ex.: Dipirona" aria-invalid={!!errors.drug} {...register("drug")} />
            </Field>
          </div>
          <div className="w-40">
            <Field label="Dose" htmlFor="med-dose" error={errors.dose?.message}>
              <Input id="med-dose" placeholder="ex.: 1 comprimido" aria-invalid={!!errors.dose} {...register("dose")} />
            </Field>
          </div>
          <Button type="submit" disabled={record.isPending}>
            <Plus /> {record.isPending ? "Registrando…" : "Registrar"}
          </Button>
        </form>
      )}
      {formError && <p className="text-sm text-destructive">{formError}</p>}

      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={<p className="rounded-lg border border-dashed py-6 text-center text-sm text-muted-foreground">Nenhuma medicação registrada.</p>}
      >
        {(meds) => (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Medicamento</TableHead>
                <TableHead>Dose</TableHead>
                <TableHead>Quando</TableHead>
                <TableHead>Por</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {meds.map((m) => (
                <TableRow key={m.id}>
                  <TableCell className="font-medium">{m.drug}</TableCell>
                  <TableCell>{m.dose}</TableCell>
                  <TableCell className="text-muted-foreground">{formatDateTime(m.administeredAt)}</TableCell>
                  <TableCell className="text-muted-foreground"><UserName userId={m.givenBy} /></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>
    </div>
  );
}
