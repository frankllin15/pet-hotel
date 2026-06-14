import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
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

const PAGE_SIZE = 20;

export function ReservationsPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState("");
  const [accommodationId, setAccommodationId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);

  // Qualquer mudança de filtro volta para a primeira página.
  const onFilter = <T,>(setter: (v: T) => void) => (v: T) => {
    setter(v);
    setPage(1);
  };

  const query = useReservations({
    status: status || undefined,
    accommodationId: accommodationId || undefined,
    from: from || undefined,
    to: to || undefined,
    page,
    pageSize: PAGE_SIZE,
  });
  const accommodationsQuery = useAccommodations();
  const actions = useReservationActions();

  const accommodationName = (id: string) =>
    accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "—";

  const hasFilters = !!(status || accommodationId || from || to);
  const clearFilters = () => {
    setStatus("");
    setAccommodationId("");
    setFrom("");
    setTo("");
    setPage(1);
  };

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
        <>
          <Select
            className="w-44"
            value={status}
            onChange={(e) => onFilter(setStatus)(e.target.value)}
            aria-label="Filtrar por status"
          >
            <option value="">Todos os status</option>
            {RESERVATION_STATUS.map((s) => (
              <option key={s} value={s}>
                {RESERVATION_STATUS_LABELS[s]}
              </option>
            ))}
          </Select>
          <Select
            className="w-52"
            value={accommodationId}
            onChange={(e) => onFilter(setAccommodationId)(e.target.value)}
            aria-label="Filtrar por acomodação"
          >
            <option value="">Todas as acomodações</option>
            {accommodationsQuery.data?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.name}
              </option>
            ))}
          </Select>
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            De
            <Input
              type="date"
              className="w-40"
              value={from}
              max={to || undefined}
              onChange={(e) => onFilter(setFrom)(e.target.value)}
              aria-label="Período de"
            />
          </label>
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            Até
            <Input
              type="date"
              className="w-40"
              value={to}
              min={from || undefined}
              onChange={(e) => onFilter(setTo)(e.target.value)}
              aria-label="Período até"
            />
          </label>
          {hasFilters && (
            <Button variant="ghost" size="sm" onClick={clearFilters}>
              Limpar filtros
            </Button>
          )}
        </>
      }
    >
      {actions.actionError && (
        <ReservationActionError message={actions.actionError} onClose={actions.clearError} />
      )}
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.items.length === 0}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhuma reserva encontrada.</p>}
      >
        {(data) => {
          const total = Number(data.total);
          const currentPage = Number(data.page);
          const totalPages = Number(data.totalPages);
          const firstRow = (currentPage - 1) * PAGE_SIZE + 1;
          const lastRow = firstRow + data.items.length - 1;

          return (
            <div className="space-y-4">
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
                  {data.items.map((r) => (
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

              <div className="flex items-center justify-between text-sm text-muted-foreground">
                <span>
                  {firstRow}–{lastRow} de {total}
                </span>
                <div className="flex items-center gap-3">
                  <span>
                    Página {currentPage} de {totalPages}
                  </span>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={currentPage <= 1}
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                    >
                      Anterior
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={currentPage >= totalPages}
                      onClick={() => setPage((p) => p + 1)}
                    >
                      Próxima
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          );
        }}
      </AsyncBoundary>

      {actions.dialog}
    </ListPage>
  );
}
