import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { formatMoney } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Card, CardContent } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Badge } from "@/shared/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import type { AccommodationDto } from "../api";
import { useAccommodations, useCreateAccommodation, useUpdateAccommodation } from "../queries";
import { accommodationFormSchema, type AccommodationFormInput } from "../schemas";

export function AccommodationsPage() {
  // null = nenhum form aberto; "new" = criação; DTO = edição daquela acomodação.
  const [editing, setEditing] = useState<AccommodationDto | "new" | null>(null);
  const query = useAccommodations();

  return (
    <ListPage
      title="Acomodações"
      description="Unidades reserváveis do hotel (boxes, suítes)."
      primaryAction={
        !editing && (
          <Button onClick={() => setEditing("new")}>
            <Plus /> Nova acomodação
          </Button>
        )
      }
    >
      {editing && (
        <AccommodationForm
          key={editing === "new" ? "new" : editing.id}
          accommodation={editing === "new" ? undefined : editing}
          onDone={() => setEditing(null)}
        />
      )}
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhuma acomodação cadastrada.</p>}
      >
        {(accommodations) => (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Diária</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {accommodations.map((a) => (
                <TableRow key={a.id}>
                  <TableCell className="font-medium">{a.name}</TableCell>
                  <TableCell>{formatMoney(a.dailyRate)}</TableCell>
                  <TableCell>
                    <Badge variant={a.status === "Available" ? "success" : "secondary"}>
                      {a.status === "Available" ? "Disponível" : "Inativa"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button size="sm" variant="outline" onClick={() => setEditing(a)}>
                      <Pencil /> Editar
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>
    </ListPage>
  );
}

function AccommodationForm({ accommodation, onDone }: { accommodation?: AccommodationDto; onDone: () => void }) {
  const isEdit = !!accommodation;
  const createMutation = useCreateAccommodation();
  const updateMutation = useUpdateAccommodation(accommodation?.id ?? "");
  const mutation = isEdit ? updateMutation : createMutation;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AccommodationFormInput>({
    resolver: zodResolver(accommodationFormSchema),
    defaultValues: accommodation
      ? { name: accommodation.name, dailyRate: Number(accommodation.dailyRate), active: accommodation.status === "Available" }
      : { name: "", dailyRate: 0, active: true },
  });

  const submit = handleSubmit((values) => {
    const done = { onSuccess: () => { reset(); onDone(); } };
    if (isEdit) {
      updateMutation.mutate(
        { id: accommodation.id, name: values.name, dailyRate: values.dailyRate, active: values.active },
        done,
      );
    } else {
      createMutation.mutate({ name: values.name, dailyRate: values.dailyRate }, done);
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Card className="mb-4">
      <CardContent className="pt-6">
        <form className="flex flex-wrap items-end gap-3" onSubmit={submit}>
          <div className="min-w-48 flex-1">
            <Field label="Nome" htmlFor="name" error={errors.name?.message}>
              <Input id="name" placeholder="Ex.: Box 1" aria-invalid={!!errors.name} {...register("name")} />
            </Field>
          </div>
          <div className="w-36">
            <Field label="Diária (R$)" htmlFor="dailyRate" error={errors.dailyRate?.message}>
              <Input
                id="dailyRate"
                type="number"
                min={0}
                step="0.01"
                placeholder="0,00"
                aria-invalid={!!errors.dailyRate}
                {...register("dailyRate", { valueAsNumber: true })}
              />
            </Field>
          </div>
          {isEdit && (
            <label className="flex h-9 items-center gap-2 text-sm">
              <input type="checkbox" className="size-4 accent-primary" {...register("active")} />
              Ativa
            </label>
          )}
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Salvar"}
          </Button>
          <Button type="button" variant="ghost" onClick={onDone}>
            Cancelar
          </Button>
        </form>
        {formError && <p className="mt-2 text-sm text-destructive">{formError}</p>}
      </CardContent>
    </Card>
  );
}
