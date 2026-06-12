import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Select } from "@/shared/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { PetName } from "../components/pet-name";
import { ReservationStatusBadge } from "../components/reservation-status-badge";
import {
  ReservationActionButtons,
  ReservationActionError,
  useReservationActions,
} from "../components/reservation-actions";
import { useAccommodations, useReservations } from "../queries";
import { ARRIVAL_CONDITION_LABELS, RESERVATION_STATUS, RESERVATION_STATUS_LABELS } from "../schemas";

export function ReservationsPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState("");

  const query = useReservations(status || undefined);
  const accommodationsQuery = useAccommodations();
  const actions = useReservationActions();

  const accommodationName = (id: string) =>
    accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "—";

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
      {actions.actionError && (
        <ReservationActionError message={actions.actionError} onClose={actions.clearError} />
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
                <TableRow
                  key={r.id}
                  data-clickable="true"
                  onClick={() => navigate(`/booking/reservations/${r.id}`)}
                >
                  <TableCell className="font-medium">
                    <PetName petId={r.petId} />
                    {r.arrivalState && (
                      <span className="mt-0.5 block text-xs font-normal text-muted-foreground">
                        Chegada: {ARRIVAL_CONDITION_LABELS[r.arrivalState.condition] ?? r.arrivalState.condition}
                        {r.arrivalState.weightKg != null && ` · ${r.arrivalState.weightKg} kg`}
                      </span>
                    )}
                  </TableCell>
                  <TableCell>{accommodationName(r.accommodationId)}</TableCell>
                  <TableCell>{formatDate(r.checkIn)}</TableCell>
                  <TableCell>{formatDate(r.checkOut)}</TableCell>
                  <TableCell>
                    <ReservationStatusBadge status={r.status} />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                      <ReservationActionButtons reservation={r} onAction={actions.trigger} busy={actions.busy} />
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>

      {actions.dialog}
    </ListPage>
  );
}
