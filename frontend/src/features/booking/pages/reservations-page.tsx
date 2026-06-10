import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { AlertTriangle, Info, LogIn, LogOut, Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Select } from "@/shared/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import type { ReservationDto } from "../api";
import { PetName } from "../components/pet-name";
import { ReservationStatusBadge } from "../components/reservation-status-badge";
import {
  useAccommodations,
  useCancelReservation,
  useCheckInReservation,
  useCheckOutReservation,
  useConfirmReservation,
  useReservations,
} from "../queries";
import { RESERVATION_STATUS, RESERVATION_STATUS_LABELS } from "../schemas";

type ReservationAction = "confirm" | "check-in" | "check-out" | "cancel";

export function ReservationsPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState("");
  const [actionError, setActionError] = useState<string | null>(null);
  // Ação pendente de confirmação no diálogo (null = fechado).
  const [pending, setPending] = useState<{ reservation: ReservationDto; action: ReservationAction } | null>(null);

  const query = useReservations(status || undefined);
  const accommodationsQuery = useAccommodations();
  const confirmMutation = useConfirmReservation();
  const checkInMutation = useCheckInReservation();
  const checkOutMutation = useCheckOutReservation();
  const cancelMutation = useCancelReservation();

  const accommodationName = (id: string) =>
    accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "—";

  const onError = (error: unknown) =>
    setActionError(error instanceof ApiError ? error.message : "Falha na operação.");

  const mutationFor = (action: ReservationAction) =>
    action === "confirm"
      ? confirmMutation
      : action === "cancel"
        ? cancelMutation
        : action === "check-out"
          ? checkOutMutation
          : checkInMutation;
  const pendingMutation = pending ? mutationFor(pending.action) : checkInMutation;

  const runPendingAction = () => {
    if (!pending) return;
    setActionError(null);
    mutationFor(pending.action).mutate(pending.reservation.id, {
      onSuccess: () => setPending(null),
      onError: (error) => {
        onError(error);
        setPending(null);
      },
    });
  };

  const busy =
    confirmMutation.isPending ||
    checkInMutation.isPending ||
    checkOutMutation.isPending ||
    cancelMutation.isPending;

  return (
    <ListPage
      title="Reservas"
      description="Solicitações e confirmações de hospedagem."
      primaryAction={
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate("/booking/accommodations")}>
            Acomodações
          </Button>
          <Button onClick={() => navigate("/booking/reservations/new")}>
            <Plus /> Nova reserva
          </Button>
        </div>
      }
      filters={
        <Select
          className="w-48"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          aria-label="Filtrar por status"
        >
          <option value="">Todos os status</option>
          {RESERVATION_STATUS.map((s) => (
            <option key={s} value={s}>
              {RESERVATION_STATUS_LABELS[s]}
            </option>
          ))}
        </Select>
      }
    >
      {actionError && (
        <div className="mb-4 flex items-center gap-2 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive">
          <AlertTriangle className="size-4 shrink-0" />
          <span className="flex-1">{actionError}</span>
          <button type="button" className="font-medium underline" onClick={() => setActionError(null)}>
            fechar
          </button>
        </div>
      )}
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhuma reserva encontrada.</p>}
      >
        {(reservations) => (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Pet</TableHead>
                <TableHead>Acomodação</TableHead>
                <TableHead>Check-in</TableHead>
                <TableHead>Check-out</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {reservations.map((r) => (
                <TableRow key={r.id}>
                  <TableCell className="font-medium">
                    <PetName petId={r.petId} />
                  </TableCell>
                  <TableCell>{accommodationName(r.accommodationId)}</TableCell>
                  <TableCell>{formatDate(r.checkIn)}</TableCell>
                  <TableCell>{formatDate(r.checkOut)}</TableCell>
                  <TableCell>
                    <ReservationStatusBadge status={r.status} />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      {r.status === "Requested" && (
                        <Button
                          size="sm"
                          disabled={busy}
                          onClick={() => setPending({ reservation: r, action: "confirm" })}
                        >
                          Confirmar
                        </Button>
                      )}
                      {r.status === "Confirmed" && (
                        <Button
                          size="sm"
                          disabled={busy}
                          onClick={() => setPending({ reservation: r, action: "check-in" })}
                        >
                          <LogIn /> Check-in
                        </Button>
                      )}
                      {r.status === "CheckedIn" && (
                        <Button
                          size="sm"
                          disabled={busy}
                          onClick={() => setPending({ reservation: r, action: "check-out" })}
                        >
                          <LogOut /> Check-out
                        </Button>
                      )}
                      {(r.status === "Requested" || r.status === "Confirmed") && (
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={busy}
                          onClick={() => setPending({ reservation: r, action: "cancel" })}
                        >
                          Cancelar
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>

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
            <ReservationActionInfo
              reservation={pending.reservation}
              action={pending.action}
              accommodationName={accommodationName(pending.reservation.accommodationId)}
            />
          )
        }
      />
    </ListPage>
  );
}

/** Título e rótulo de confirmação por ação. */
const ACTION_META: Record<ReservationAction, { title: string; confirmLabel: string }> = {
  confirm: { title: "Confirmar reserva", confirmLabel: "Confirmar reserva" },
  "check-in": { title: "Confirmar check-in", confirmLabel: "Registrar check-in" },
  "check-out": { title: "Confirmar check-out", confirmLabel: "Registrar check-out" },
  cancel: { title: "Cancelar reserva", confirmLabel: "Cancelar reserva" },
};

/** Conteúdo didático do diálogo: contexto da reserva + consequências de cada ação. */
function ReservationActionInfo({
  reservation,
  action,
  accommodationName,
}: {
  reservation: ReservationDto;
  action: ReservationAction;
  accommodationName: string;
}) {
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
