import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { ApiError } from "@/shared/lib/problem-details";
import { formatMoney } from "@/shared/lib/format";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { listPets } from "@/features/registry/api";
import { registryKeys } from "@/features/registry/queries";
import { useAccommodations, useCreateReservation, useSharingCompatibility } from "../queries";
import { SharingCompatibilityWarning } from "../components/sharing-compatibility-warning";
import { reservationFormSchema, type ReservationFormInput } from "../schemas";

export function ReservationFormPage() {
  const navigate = useNavigate();
  const mutation = useCreateReservation();

  const petsQuery = useQuery({
    queryKey: [...registryKeys.pets({}), "selector"],
    queryFn: () => listPets({ limit: 100 }),
  });
  const accommodationsQuery = useAccommodations();
  const available = accommodationsQuery.data?.filter((a) => a.status === "Available") ?? [];

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ReservationFormInput>({
    resolver: zodResolver(reservationFormSchema),
    defaultValues: { petId: "", accommodationId: "", checkIn: "", checkOut: "" },
  });

  // Total estimado (diária da acomodação × noites) — atualiza conforme a seleção.
  const [petId, accommodationId, checkIn, checkOut] = useWatch({
    control,
    name: ["petId", "accommodationId", "checkIn", "checkOut"],
  });
  const selected = available.find((a) => a.id === accommodationId);
  const nights =
    checkIn && checkOut && checkOut > checkIn
      ? Math.round((new Date(checkOut).getTime() - new Date(checkIn).getTime()) / 86_400_000)
      : 0;
  const estimatedTotal = selected && nights > 0 ? Number(selected.dailyRate) * nights : null;

  // Alerta (não-bloqueante) de compatibilidade ao dividir a acomodação no período.
  const compatibility = useSharingCompatibility({ accommodationId, checkIn, checkOut, petId });

  const submit = handleSubmit((values) =>
    mutation.mutate(values, {
      onSuccess: () => navigate("/booking/reservations", { replace: true }),
    }),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <FormPage
      title="Nova reserva"
      description="Solicite uma hospedagem (fica como Solicitada até a confirmação)."
      onSubmit={submit}
      footer={
        <>
          <Button type="button" variant="ghost" onClick={() => navigate("/booking/reservations")}>
            Cancelar
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Solicitar reserva"}
          </Button>
        </>
      }
    >
      <Field label="Pet" htmlFor="petId" error={errors.petId?.message}>
        <Select id="petId" aria-invalid={!!errors.petId} defaultValue="" {...register("petId")}>
          <option value="" disabled>
            {petsQuery.isPending ? "Carregando pets…" : "Selecione um pet"}
          </option>
          {petsQuery.data?.items.map((pet) => (
            <option key={pet.id} value={pet.id}>
              {pet.name}
            </option>
          ))}
        </Select>
      </Field>

      <Field label="Acomodação" htmlFor="accommodationId" error={errors.accommodationId?.message}>
        <Select
          id="accommodationId"
          aria-invalid={!!errors.accommodationId}
          defaultValue=""
          {...register("accommodationId")}
        >
          <option value="" disabled>
            {accommodationsQuery.isPending ? "Carregando acomodações…" : "Selecione uma acomodação"}
          </option>
          {available.map((a) => (
            <option key={a.id} value={a.id}>
              {a.name} — {formatMoney(a.dailyRate)}/diária · até {Number(a.capacity)} {Number(a.capacity) === 1 ? "pet" : "pets"}
            </option>
          ))}
        </Select>
      </Field>

      <div className="grid grid-cols-2 gap-4">
        <Field label="Check-in" htmlFor="checkIn" error={errors.checkIn?.message}>
          <Input id="checkIn" type="date" aria-invalid={!!errors.checkIn} {...register("checkIn")} />
        </Field>
        <Field label="Check-out" htmlFor="checkOut" error={errors.checkOut?.message}>
          <Input id="checkOut" type="date" aria-invalid={!!errors.checkOut} {...register("checkOut")} />
        </Field>
      </div>

      {estimatedTotal !== null && (
        <div className="flex items-center justify-between rounded-lg border border-border bg-muted/40 px-4 py-3 text-sm">
          <span className="text-muted-foreground">
            Total estimado · {nights} {nights === 1 ? "noite" : "noites"} × {formatMoney(selected!.dailyRate)}
          </span>
          <span className="font-display text-lg font-semibold">{formatMoney(estimatedTotal)}</span>
        </div>
      )}

      <SharingCompatibilityWarning data={compatibility.data} />

      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}
