import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { AlertTriangle, Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Select } from "@/shared/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { PetName } from "../components/pet-name";
import { ReservationStatusBadge } from "../components/reservation-status-badge";
import { useAccommodations, useCancelReservation, useConfirmReservation, useReservations } from "../queries";
import { RESERVATION_STATUS, RESERVATION_STATUS_LABELS } from "../schemas";

export function ReservationsPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState("");
  const [actionError, setActionError] = useState<string | null>(null);

  const query = useReservations(status || undefined);
  const accommodationsQuery = useAccommodations();
  const confirmMutation = useConfirmReservation();
  const cancelMutation = useCancelReservation();

  const accommodationName = (id: string) =>
    accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "—";

  const onError = (error: unknown) =>
    setActionError(error instanceof ApiError ? error.message : "Falha na operação.");

  const busy = confirmMutation.isPending || cancelMutation.isPending;

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
                          onClick={() => {
                            setActionError(null);
                            confirmMutation.mutate(r.id, { onError });
                          }}
                        >
                          Confirmar
                        </Button>
                      )}
                      {r.status !== "Cancelled" && (
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={busy}
                          onClick={() => {
                            setActionError(null);
                            cancelMutation.mutate(r.id, { onError });
                          }}
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
    </ListPage>
  );
}
