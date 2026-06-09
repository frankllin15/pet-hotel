import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Plus } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Card, CardContent } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Badge } from "@/shared/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { useAccommodations, useCreateAccommodation } from "../queries";
import { accommodationFormSchema, type AccommodationFormInput } from "../schemas";

export function AccommodationsPage() {
  const [showForm, setShowForm] = useState(false);
  const query = useAccommodations();

  return (
    <ListPage
      title="Acomodações"
      description="Unidades reserváveis do hotel (boxes, suítes)."
      primaryAction={
        !showForm && (
          <Button onClick={() => setShowForm(true)}>
            <Plus /> Nova acomodação
          </Button>
        )
      }
    >
      {showForm && <NewAccommodationForm onDone={() => setShowForm(false)} />}
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
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {accommodations.map((a) => (
                <TableRow key={a.id}>
                  <TableCell className="font-medium">{a.name}</TableCell>
                  <TableCell>
                    <Badge variant={a.status === "Available" ? "success" : "secondary"}>
                      {a.status === "Available" ? "Disponível" : "Inativa"}
                    </Badge>
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

function NewAccommodationForm({ onDone }: { onDone: () => void }) {
  const mutation = useCreateAccommodation();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AccommodationFormInput>({ resolver: zodResolver(accommodationFormSchema) });

  const submit = handleSubmit((values) =>
    mutation.mutate(values.name, {
      onSuccess: () => {
        reset();
        onDone();
      },
    }),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Card className="mb-4">
      <CardContent className="pt-6">
        <form className="flex items-end gap-3" onSubmit={submit}>
          <div className="flex-1">
            <Field label="Nome" htmlFor="name" error={errors.name?.message}>
              <Input id="name" placeholder="Ex.: Box 1" aria-invalid={!!errors.name} {...register("name")} />
            </Field>
          </div>
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
