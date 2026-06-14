import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { formatMoney } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Modal } from "@/shared/ui/modal";
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
        <Button onClick={() => setEditing("new")}>
          <Plus /> Nova acomodação
        </Button>
      }
    >
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
                <TableHead>Capacidade</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {accommodations.map((a) => (
                <TableRow key={a.id}>
                  <TableCell className="font-medium">{a.name}</TableCell>
                  <TableCell>{formatMoney(a.dailyRate)}</TableCell>
                  <TableCell>{Number(a.capacity)} {Number(a.capacity) === 1 ? "pet" : "pets"}</TableCell>
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

      {editing && (
        <AccommodationFormModal
          key={editing === "new" ? "new" : editing.id}
          accommodation={editing === "new" ? undefined : editing}
          onClose={() => setEditing(null)}
        />
      )}
    </ListPage>
  );
}

function AccommodationFormModal({ accommodation, onClose }: { accommodation?: AccommodationDto; onClose: () => void }) {
  const isEdit = !!accommodation;
  const createMutation = useCreateAccommodation();
  const updateMutation = useUpdateAccommodation(accommodation?.id ?? "");
  const mutation = isEdit ? updateMutation : createMutation;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AccommodationFormInput>({
    resolver: zodResolver(accommodationFormSchema),
    defaultValues: accommodation
      ? {
          name: accommodation.name,
          dailyRate: Number(accommodation.dailyRate),
          capacity: Number(accommodation.capacity),
          active: accommodation.status === "Available",
        }
      : { name: "", dailyRate: 0, capacity: 1, active: true },
  });

  const submit = handleSubmit((values) => {
    if (isEdit) {
      updateMutation.mutate(
        { id: accommodation.id, name: values.name, dailyRate: values.dailyRate, capacity: values.capacity, active: values.active },
        { onSuccess: onClose },
      );
    } else {
      createMutation.mutate({ name: values.name, dailyRate: values.dailyRate, capacity: values.capacity }, { onSuccess: onClose });
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Modal
      open
      title={isEdit ? "Editar acomodação" : "Nova acomodação"}
      description="Unidade reservável do hotel (box, suíte)."
      busy={mutation.isPending}
      onClose={onClose}
    >
      <form className="space-y-4" onSubmit={submit}>
        <Field label="Nome" htmlFor="name" error={errors.name?.message}>
          <Input id="name" placeholder="Ex.: Box 1" aria-invalid={!!errors.name} {...register("name")} />
        </Field>

        <div className="grid grid-cols-2 gap-3">
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
          <Field label="Capacidade (pets)" htmlFor="capacity" error={errors.capacity?.message}>
            <Input
              id="capacity"
              type="number"
              min={1}
              step="1"
              placeholder="1"
              aria-invalid={!!errors.capacity}
              {...register("capacity", { valueAsNumber: true })}
            />
          </Field>
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" className="size-4 accent-primary" {...register("active")} />
            Ativa (disponível para reservas)
          </label>
        )}

        {formError && <p className="text-sm text-destructive">{formError}</p>}

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            Cancelar
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Salvar"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
