import { useMemo, useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { addDays, eachDay, startOfToday, toISODate } from "@/shared/lib/dates";
import { cn } from "@/shared/lib/utils";
import { AsyncBoundary, type QueryLike } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { DashboardPage } from "@/shared/ui/archetypes/dashboard-page";
import type { AccommodationDto, OccupancyEntryDto } from "../api";
import { PetName } from "../components/pet-name";
import { useAccommodations, useOccupancy } from "../queries";

const WINDOW_DAYS = 14;
const dayLabel = new Intl.DateTimeFormat("pt-BR", { weekday: "short", day: "2-digit" });

export function OccupancyPage() {
  const [start, setStart] = useState(startOfToday);

  const days = useMemo(() => eachDay(start, WINDOW_DAYS), [start]);
  const from = toISODate(start);
  const to = toISODate(addDays(start, WINDOW_DAYS));

  const occupancyQuery = useOccupancy(from, to);
  const accommodationsQuery = useAccommodations();

  return (
    <DashboardPage
      title="Ocupação"
      description="Reservas confirmadas por acomodação."
      actions={
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => setStart((s) => addDays(s, -WINDOW_DAYS))}>
            <ChevronLeft /> Anterior
          </Button>
          <Button variant="outline" size="sm" onClick={() => setStart(startOfToday())}>
            Hoje
          </Button>
          <Button variant="outline" size="sm" onClick={() => setStart((s) => addDays(s, WINDOW_DAYS))}>
            Próximo <ChevronRight />
          </Button>
        </div>
      }
    >
      <AsyncBoundary query={accommodationsQuery} isEmpty={(d) => d.length === 0}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Cadastre acomodações para ver a ocupação.</p>}
      >
        {(accommodations) => (
          <AsyncBoundary query={occupancyQuery as QueryLike<OccupancyEntryDto[]>} isEmpty={() => false}>
            {(occupancy) => (
              <OccupancyGrid accommodations={accommodations} occupancy={occupancy} days={days} />
            )}
          </AsyncBoundary>
        )}
      </AsyncBoundary>
    </DashboardPage>
  );
}

function OccupancyGrid({
  accommodations,
  occupancy,
  days,
}: {
  accommodations: AccommodationDto[];
  occupancy: OccupancyEntryDto[];
  days: Date[];
}) {
  const dayKeys = days.map(toISODate);

  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b bg-muted/50">
            <th className="sticky left-0 z-10 bg-muted/50 px-3 py-2 text-left font-medium">Acomodação</th>
            {days.map((d) => (
              <th key={toISODate(d)} className="min-w-24 px-2 py-2 text-center font-medium text-muted-foreground">
                {dayLabel.format(d)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {accommodations.map((acc) => {
            const entries = occupancy.filter((e) => e.accommodationId === acc.id);
            return (
              <tr key={acc.id} className="border-b last:border-0">
                <td className="sticky left-0 z-10 bg-background px-3 py-2 font-medium">{acc.name}</td>
                {dayKeys.map((dayKey) => {
                  const entry = entries.find((e) => e.checkIn <= dayKey && dayKey < e.checkOut);
                  // Mostra o nome só no início da estadia (ou na 1ª coluna visível).
                  const showName = entry && (entry.checkIn === dayKey || dayKey === dayKeys[0]);
                  return (
                    <td key={dayKey} className="px-1 py-1.5">
                      <div
                        className={cn(
                          "flex h-7 items-center justify-center truncate rounded px-1 text-xs",
                          entry ? "bg-primary/15 text-foreground" : "",
                        )}
                      >
                        {showName ? <PetName petId={entry.petId} /> : null}
                      </div>
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
