import { useMemo, useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { addMonths, isSameMonth, startOfMonth, startOfToday, toISODate } from "@/shared/lib/dates";
import { formatDate } from "@/shared/lib/format";
import { cn } from "@/shared/lib/utils";
import { AsyncBoundary, type QueryLike } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { DashboardPage } from "@/shared/ui/archetypes/dashboard-page";
import type { OccupancyEntryDto } from "../api";
import { PetName } from "../components/pet-name";
import { useAccommodations, useOccupancy } from "../queries";
import {
  accommodationColor,
  buildMonthGrid,
  type CalendarEvent,
  type CalendarWeek,
} from "../lib/month-calendar";

const WEEKDAYS = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];
const monthFormatter = new Intl.DateTimeFormat("pt-BR", { month: "long", year: "numeric" });

// Métricas do layout (px) — barras posicionadas em lanes dentro de cada semana.
const HEADER_OFFSET = 28;
const BAR_HEIGHT = 20;
const BAR_GAP = 4;
const WEEK_PADDING = 8;

export function OccupancyPage() {
  const [month, setMonth] = useState(() => startOfMonth(startOfToday()));

  // Intervalo da grade (independe dos eventos) -> alimenta a query.
  const range = useMemo(() => buildMonthGrid(month, []), [month]);
  const from = toISODate(range.gridStart);
  const to = toISODate(range.gridEndExclusive);

  const occupancyQuery = useOccupancy(from, to);
  const accommodationsQuery = useAccommodations();
  const accommodationName = (id: string) =>
    accommodationsQuery.data?.find((a) => a.id === id)?.name ?? "";

  const label = monthFormatter.format(month);

  return (
    <DashboardPage
      title="Ocupação"
      description="Reservas confirmadas no mês."
      actions={
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" aria-label="Mês anterior" onClick={() => setMonth((m) => addMonths(m, -1))}>
            <ChevronLeft />
          </Button>
          <Button variant="outline" size="sm" onClick={() => setMonth(startOfMonth(startOfToday()))}>
            Hoje
          </Button>
          <Button variant="outline" size="icon" aria-label="Próximo mês" onClick={() => setMonth((m) => addMonths(m, 1))}>
            <ChevronRight />
          </Button>
        </div>
      }
    >
      <div className="flex items-center gap-3">
        <h2 className="text-lg font-semibold capitalize">{label}</h2>
      </div>

      <AsyncBoundary query={occupancyQuery as QueryLike<OccupancyEntryDto[]>} isEmpty={() => false}>
        {(occupancy) => {
          const events: CalendarEvent[] = occupancy.map((e) => ({
            id: e.reservationId,
            petId: e.petId,
            accommodationId: e.accommodationId,
            start: e.checkIn,
            end: e.checkOut,
          }));
          const grid = buildMonthGrid(month, events);
          // Acomodações presentes no mês -> legenda de cores (ordenadas por nome).
          const legend = [...new Set(events.map((e) => e.accommodationId))]
            .map((id) => ({ id, name: accommodationName(id) }))
            .sort((a, b) => a.name.localeCompare(b.name));

          return (
            <>
              <div className="overflow-hidden rounded-lg border">
                <div className="grid grid-cols-7 border-b bg-muted/40">
                  {WEEKDAYS.map((wd) => (
                    <div key={wd} className="px-2 py-2 text-center text-xs font-medium text-muted-foreground">
                      {wd}
                    </div>
                  ))}
                </div>
                {grid.weeks.map((week, i) => (
                  <WeekRow
                    key={i}
                    week={week}
                    month={month}
                    accommodationName={accommodationName}
                  />
                ))}
              </div>
              <CalendarLegend items={legend} />
            </>
          );
        }}
      </AsyncBoundary>
    </DashboardPage>
  );
}

/** Legenda de cores: cada acomodação presente no mês com seu swatch (cor estável por Id). */
function CalendarLegend({ items }: { items: { id: string; name: string }[] }) {
  if (items.length === 0) return null;

  return (
    <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-2">
      <span className="text-xs font-medium text-muted-foreground">Acomodações:</span>
      {items.map((item) => (
        <span key={item.id} className="flex items-center gap-1.5 text-xs">
          <span className={cn("size-3 shrink-0 rounded-sm", accommodationColor(item.id))} aria-hidden />
          {item.name || "—"}
        </span>
      ))}
    </div>
  );
}

function WeekRow({
  week,
  month,
  accommodationName,
}: {
  week: CalendarWeek;
  month: Date;
  accommodationName: (id: string) => string;
}) {
  const todayISO = toISODate(startOfToday());
  const minHeight = HEADER_OFFSET + Math.max(week.laneCount, 1) * (BAR_HEIGHT + BAR_GAP) + WEEK_PADDING;

  return (
    <div className="relative border-b last:border-b-0" style={{ minHeight }}>
      {/* Fundo: 7 células com o número do dia. */}
      <div className="grid h-full grid-cols-7">
        {week.days.map((day) => {
          const outside = !isSameMonth(day, month);
          const isToday = toISODate(day) === todayISO;
          return (
            <div key={toISODate(day)} className={cn("border-r p-1 last:border-r-0", outside && "bg-muted/20")}>
              <span
                className={cn(
                  "flex size-6 items-center justify-center rounded-full text-xs",
                  outside && "text-muted-foreground/50",
                  isToday && "bg-primary font-semibold text-primary-foreground",
                )}
              >
                {day.getDate()}
              </span>
            </div>
          );
        })}
      </div>

      {/* Sobreposição: barras dos eventos atravessando os dias. */}
      <div className="pointer-events-none absolute inset-x-0" style={{ top: HEADER_OFFSET }}>
        {week.segments.map((seg) => (
          <div
            key={`${seg.event.id}-${seg.colStart}`}
            className="absolute px-0.5"
            style={{
              left: `${(seg.colStart / 7) * 100}%`,
              width: `${(seg.span / 7) * 100}%`,
              top: seg.lane * (BAR_HEIGHT + BAR_GAP),
              height: BAR_HEIGHT,
            }}
          >
            <div
              title={`${accommodationName(seg.event.accommodationId)} · ${formatDate(seg.event.start)} → ${formatDate(seg.event.end)}`}
              className={cn(
                "flex h-full items-center truncate rounded px-1.5 text-xs font-medium",
                accommodationColor(seg.event.accommodationId),
                !seg.roundedLeft && "rounded-l-none",
                !seg.roundedRight && "rounded-r-none",
              )}
            >
              <PetName petId={seg.event.petId} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
