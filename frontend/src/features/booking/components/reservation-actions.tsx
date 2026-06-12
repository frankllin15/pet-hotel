import { useState, type ReactNode } from "react";
import { AlertTriangle, Info, LogIn, LogOut } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { Button, type ButtonProps } from "@/shared/ui/button";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { Select } from "@/shared/ui/select";
import { Textarea } from "@/shared/ui/textarea";
import type { ArrivalStateInput, ReservationDto } from "../api";
import {
  useAccommodations,
  useCancelReservation,
  useCheckInReservation,
  useCheckOutReservation,
  useConfirmReservation,
} from "../queries";
import { ARRIVAL_CONDITIONS, ARRIVAL_CONDITION_LABELS } from "../schemas";
import { PetName } from "./pet-name";

export type ReservationAction = "confirm" | "check-in" | "check-out" | "cancel";

/** Estado do form de chegada (peso e observação livres; condição sempre preenchida). */
type ArrivalForm = { weightKg: string; condition: string; observations: string };
const EMPTY_ARRIVAL: ArrivalForm = { weightKg: "", condition: "Healthy", observations: "" };

/** Título e rótulo de confirmação por ação. */
const ACTION_META: Record<ReservationAction, { title: string; confirmLabel: string }> = {
  confirm: { title: "Confirmar reserva", confirmLabel: "Confirmar reserva" },
  "check-in": { title: "Confirmar check-in", confirmLabel: "Registrar check-in" },
  "check-out": { title: "Confirmar check-out", confirmLabel: "Registrar check-out" },
  cancel: { title: "Cancelar reserva", confirmLabel: "Cancelar reserva" },
};

/**
 * Centraliza as ações de ciclo de vida da reserva (confirmar/check-in/check-out/cancelar):
 * mutações, diálogo de confirmação com texto didático e o form de estado de chegada no
 * check-in. Reutilizado pela lista e pela ficha da reserva — basta renderizar `dialog` uma
 * vez e disparar `trigger(reservation, action)` (via <ReservationActionButtons/>).
 */
export function useReservationActions() {
  const [actionError, setActionError] = useState<string | null>(null);
  const [pending, setPending] = useState<{ reservation: ReservationDto; action: ReservationAction } | null>(null);
  const [arrival, setArrival] = useState<ArrivalForm>(EMPTY_ARRIVAL);

  const accommodationsQuery = useAccommodations();
  const confirmMutation = useConfirmReservation();
  const checkInMutation = useCheckInReservation();
  const checkOutMutation = useCheckOutReservation();
  const cancelMutation = useCancelReservation();

  const accommodationName = (id: string) => accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "—";
  const onError = (error: unknown) =>
    setActionError(error instanceof ApiError ? error.message : "Falha na operação.");

  // Ações que só precisam do Id (check-in é tratado à parte por carregar o estado de chegada).
  const mutationFor = (action: Exclude<ReservationAction, "check-in">) =>
    action === "confirm" ? confirmMutation : action === "cancel" ? cancelMutation : checkOutMutation;
  const pendingMutation =
    pending?.action === "check-in" ? checkInMutation : pending ? mutationFor(pending.action) : confirmMutation;

  const trigger = (reservation: ReservationDto, action: ReservationAction) => {
    if (action === "check-in") setArrival(EMPTY_ARRIVAL);
    setActionError(null);
    setPending({ reservation, action });
  };

  const runPendingAction = () => {
    if (!pending) return;
    setActionError(null);
    const callbacks = {
      onSuccess: () => setPending(null),
      onError: (error: unknown) => {
        onError(error);
        setPending(null);
      },
    };

    if (pending.action === "check-in") {
      const arrivalState: ArrivalStateInput = {
        weightKg: arrival.weightKg ? Number(arrival.weightKg) : null,
        condition: arrival.condition as ArrivalStateInput["condition"],
        observations: arrival.observations ? arrival.observations : null,
      };
      checkInMutation.mutate({ id: pending.reservation.id, arrivalState }, callbacks);
    } else {
      mutationFor(pending.action).mutate(pending.reservation.id, callbacks);
    }
  };

  const busy =
    confirmMutation.isPending ||
    checkInMutation.isPending ||
    checkOutMutation.isPending ||
    cancelMutation.isPending;

  const dialog = (
    <ConfirmDialog
      open={pending !== null}
      loading={pendingMutation.isPending}
      title={pending ? ACTION_META[pending.action].title : ""}
      confirmLabel={pending ? ACTION_META[pending.action].confirmLabel : ""}
      confirmVariant={pending?.action === "cancel" ? "destructive" : "default"}
      cancelLabel="Voltar"
      onConfirm={runPendingAction}
      onClose={() => {
        if (!pendingMutation.isPending) setPending(null);
      }}
      description={
        pending && (
          <div className="space-y-3">
            <ReservationActionInfo
              reservation={pending.reservation}
              action={pending.action}
              accommodationName={accommodationName(pending.reservation.accommodationId)}
            />
            {pending.action === "check-in" && (
              <ArrivalStateForm value={arrival} onChange={setArrival} disabled={checkInMutation.isPending} />
            )}
          </div>
        )
      }
    />
  );

  return { trigger, busy, actionError, clearError: () => setActionError(null), dialog };
}

/** Botões de ação disponíveis conforme o status da reserva. Vazio quando não há ação possível. */
export function ReservationActionButtons({
  reservation,
  onAction,
  busy,
  size = "sm",
}: {
  reservation: ReservationDto;
  onAction: (reservation: ReservationDto, action: ReservationAction) => void;
  busy: boolean;
  size?: ButtonProps["size"];
}) {
  return (
    <>
      {reservation.status === "Requested" && (
        <Button size={size} disabled={busy} onClick={() => onAction(reservation, "confirm")}>
          Confirmar
        </Button>
      )}
      {reservation.status === "Confirmed" && (
        <Button size={size} disabled={busy} onClick={() => onAction(reservation, "check-in")}>
          <LogIn /> Check-in
        </Button>
      )}
      {reservation.status === "CheckedIn" && (
        <Button size={size} disabled={busy} onClick={() => onAction(reservation, "check-out")}>
          <LogOut /> Check-out
        </Button>
      )}
      {(reservation.status === "Requested" || reservation.status === "Confirmed") && (
        <Button size={size} variant="outline" disabled={busy} onClick={() => onAction(reservation, "cancel")}>
          Cancelar
        </Button>
      )}
    </>
  );
}

/** Banner de erro de uma ação (409 de vacina, estado inválido, etc.). */
export function ReservationActionError({ message, onClose }: { message: string; onClose: () => void }) {
  return (
    <div className="mb-4 flex items-center gap-2 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive">
      <AlertTriangle className="size-4 shrink-0" />
      <span className="flex-1">{message}</span>
      <button type="button" className="font-medium underline" onClick={onClose}>
        fechar
      </button>
    </div>
  );
}

/** Form do estado de chegada no check-in (condição obrigatória; peso e observação opcionais). */
function ArrivalStateForm({
  value,
  onChange,
  disabled,
}: {
  value: ArrivalForm;
  onChange: (next: ArrivalForm) => void;
  disabled?: boolean;
}) {
  return (
    <div className="space-y-3 rounded-md border border-border bg-card px-3 py-3">
      <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">Estado de chegada</p>
      <div className="grid gap-3 sm:grid-cols-[1fr_7rem]">
        <Field label="Condição" htmlFor="arrival-condition">
          <Select
            id="arrival-condition"
            value={value.condition}
            disabled={disabled}
            onChange={(e) => onChange({ ...value, condition: e.target.value })}
          >
            {ARRIVAL_CONDITIONS.map((c) => (
              <option key={c} value={c}>
                {ARRIVAL_CONDITION_LABELS[c]}
              </option>
            ))}
          </Select>
        </Field>
        <Field label="Peso (kg)" htmlFor="arrival-weight">
          <Input
            id="arrival-weight"
            type="number"
            min={0}
            step="0.1"
            placeholder="opcional"
            value={value.weightKg}
            disabled={disabled}
            onChange={(e) => onChange({ ...value, weightKg: e.target.value })}
          />
        </Field>
      </div>
      <Field label="Observações" htmlFor="arrival-observations">
        <Textarea
          id="arrival-observations"
          rows={2}
          placeholder="ex.: chegou agitado, pequeno corte na pata"
          value={value.observations}
          disabled={disabled}
          onChange={(e) => onChange({ ...value, observations: e.target.value })}
        />
      </Field>
    </div>
  );
}

/** Conteúdo didático do diálogo: contexto da reserva + consequências de cada ação. */
function ReservationActionInfo({
  reservation,
  action,
  accommodationName,
}: {
  reservation: ReservationDto;
  action: ReservationAction;
  accommodationName: string;
}): ReactNode {
  const pet = (
    <span className="font-medium text-foreground">
      <PetName petId={reservation.petId} />
    </span>
  );
  const room = <span className="font-medium text-foreground">{accommodationName}</span>;
  const period = `${formatDate(reservation.checkIn)} → ${formatDate(reservation.checkOut)}`;

  if (action === "confirm") {
    return (
      <>
        <p>
          Você está <strong className="text-foreground">confirmando</strong> a reserva de {pet} na acomodação {room} (
          {period}).
        </p>
        <p>
          A acomodação fica <strong className="text-foreground">garantida</strong> para esse período (bloqueando
          conflitos com outras reservas).
        </p>
        <div className="flex gap-2 rounded-md border border-border bg-muted/40 px-3 py-2 text-foreground">
          <Info className="mt-0.5 size-4 shrink-0 text-muted-foreground" />
          <span>
            A confirmação valida a <strong>aptidão sanitária</strong> do pet: se uma vacina obrigatória estiver ausente
            ou vencida na data do check-in, a operação será <strong>bloqueada</strong> até a regularização.
          </span>
        </div>
      </>
    );
  }

  if (action === "check-in") {
    return (
      <>
        <p>
          Você está registrando a <strong className="text-foreground">entrada</strong> de {pet} na acomodação {room}.
        </p>
        <p>
          A estadia passa a valer <strong className="text-foreground">a partir de agora</strong> e a acomodação fica
          marcada como ocupada no período reservado ({period}).
        </p>
        <div className="flex gap-2 rounded-md border border-warning/40 bg-warning/10 px-3 py-2 text-foreground">
          <AlertTriangle className="mt-0.5 size-4 shrink-0 text-warning" />
          <span>
            Depois do check-in a reserva <strong>não pode mais ser cancelada</strong> — ela só será encerrada quando
            você registrar o check-out.
          </span>
        </div>
      </>
    );
  }

  if (action === "check-out") {
    return (
      <>
        <p>
          Você está registrando a <strong className="text-foreground">saída</strong> de {pet} da acomodação {room}.
        </p>
        <p>
          Isto <strong className="text-foreground">encerra a estadia agora</strong> e libera a acomodação para receber
          novas reservas no período.
        </p>
        {reservation.arrivalState && (
          <div className="rounded-md border border-border bg-muted/40 px-3 py-2 text-foreground">
            <p className="mb-1 text-xs font-medium uppercase tracking-wider text-muted-foreground">
              Estado registrado na chegada
            </p>
            <p>
              {ARRIVAL_CONDITION_LABELS[reservation.arrivalState.condition] ?? reservation.arrivalState.condition}
              {reservation.arrivalState.weightKg != null && ` · ${reservation.arrivalState.weightKg} kg`}
            </p>
            {reservation.arrivalState.observations && (
              <p className="mt-1 text-muted-foreground">{reservation.arrivalState.observations}</p>
            )}
          </div>
        )}
        <div className="flex gap-2 rounded-md border border-border bg-muted/40 px-3 py-2 text-foreground">
          <Info className="mt-0.5 size-4 shrink-0 text-muted-foreground" />
          <span>
            Esta ação <strong>finaliza a reserva</strong> e não pode ser desfeita. Confirme apenas quando o pet já
            tiver deixado o hotel.
          </span>
        </div>
      </>
    );
  }

  return (
    <>
      <p>
        Você está <strong className="text-foreground">cancelando</strong> a reserva de {pet} na acomodação {room} (
        {period}).
      </p>
      <p>
        A acomodação volta a ficar <strong className="text-foreground">livre</strong> nesse período, disponível para
        novas reservas.
      </p>
      <div className="flex gap-2 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-foreground">
        <AlertTriangle className="mt-0.5 size-4 shrink-0 text-destructive" />
        <span>
          Esta ação <strong>não pode ser desfeita</strong>. Só é possível cancelar antes do check-in; uma vez iniciada
          a estadia, a reserva precisa ser encerrada via check-out.
        </span>
      </div>
    </>
  );
}
