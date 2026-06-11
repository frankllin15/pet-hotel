import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import type { components } from "@/shared/api/schema";
import { useSetVetContact } from "../queries";
import { vetContactFormSchema, type VetContactFormInput } from "../schemas";

type VetContactDto = components["schemas"]["VetContactDto"];

/** Veterinário particular do pet: exibição + edição inline (define/substitui via PUT). */
export function VetContactCard({ petId, vetContact }: { petId: string; vetContact: VetContactDto | null }) {
  const [editing, setEditing] = useState(false);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm">Veterinário particular</CardTitle>
        {!editing && (
          <Button size="sm" variant="outline" onClick={() => setEditing(true)}>
            {vetContact ? (
              <>
                <Pencil /> Editar
              </>
            ) : (
              <>
                <Plus /> Informar
              </>
            )}
          </Button>
        )}
      </CardHeader>
      <CardContent className="text-sm">
        {editing ? (
          <VetContactForm petId={petId} vetContact={vetContact} onDone={() => setEditing(false)} />
        ) : vetContact ? (
          <div className="space-y-1">
            <p className="font-medium">{vetContact.name}</p>
            <p className="text-muted-foreground">{vetContact.phone}</p>
            {vetContact.clinic && <p className="text-muted-foreground">{vetContact.clinic}</p>}
          </div>
        ) : (
          <p className="text-muted-foreground">Nenhum veterinário informado.</p>
        )}
      </CardContent>
    </Card>
  );
}

function VetContactForm({
  petId,
  vetContact,
  onDone,
}: {
  petId: string;
  vetContact: VetContactDto | null;
  onDone: () => void;
}) {
  const mutation = useSetVetContact(petId);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<VetContactFormInput>({
    resolver: zodResolver(vetContactFormSchema),
    defaultValues: {
      name: vetContact?.name ?? "",
      phone: vetContact?.phone ?? "",
      clinic: vetContact?.clinic ?? "",
    },
  });

  const submit = handleSubmit((values) =>
    mutation.mutate(
      { name: values.name, phone: values.phone, clinic: values.clinic ? values.clinic : null },
      { onSuccess: onDone },
    ),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <form className="space-y-3" onSubmit={submit}>
      <Field label="Nome" htmlFor="vet-name" error={errors.name?.message}>
        <Input id="vet-name" aria-invalid={!!errors.name} {...register("name")} />
      </Field>
      <Field label="Telefone" htmlFor="vet-phone" error={errors.phone?.message}>
        <Input id="vet-phone" aria-invalid={!!errors.phone} {...register("phone")} />
      </Field>
      <Field label="Clínica" htmlFor="vet-clinic" error={errors.clinic?.message}>
        <Input id="vet-clinic" {...register("clinic")} />
      </Field>
      {formError && <p className="text-sm text-destructive">{formError}</p>}
      <div className="flex justify-end gap-2">
        <Button type="button" variant="ghost" onClick={onDone}>
          Cancelar
        </Button>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Salvando…" : "Salvar"}
        </Button>
      </div>
    </form>
  );
}
