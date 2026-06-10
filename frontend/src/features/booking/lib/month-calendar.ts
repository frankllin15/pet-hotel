import { addDays, diffDays, eachDay, startOfMonth, startOfWeekSunday, toISODate } from "@/shared/lib/dates";

/** Evento do calendário (uma reserva). `end` é exclusivo (check-out). */
export interface CalendarEvent {
  id: string;
  petId: string;
  accommodationId: string;
  start: string;
  end: string;
}

/** Segmento de um evento dentro de uma semana, já posicionado em coluna/lane. */
export interface PlacedSegment {
  event: CalendarEvent;
  colStart: number; // 0..6
  span: number; // 1..7
  lane: number;
  roundedLeft: boolean; // o evento começa neste segmento
  roundedRight: boolean; // o evento termina neste segmento
}

export interface CalendarWeek {
  days: Date[];
  segments: PlacedSegment[];
  laneCount: number;
}

export interface MonthGrid {
  weeks: CalendarWeek[];
  gridStart: Date;
  gridEndExclusive: Date;
}

/** Constrói a grade do mês (semanas iniciando no domingo) com os eventos posicionados. */
export function buildMonthGrid(month: Date, events: CalendarEvent[]): MonthGrid {
  const monthStart = startOfMonth(month);
  const gridStart = startOfWeekSunday(monthStart);
  const monthEnd = addDays(startOfMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1)), -1);

  const spanDays = diffDays(toISODate(gridStart), toISODate(monthEnd)) + 1;
  const weekCount = Math.ceil(spanDays / 7);
  const gridEndExclusive = addDays(gridStart, weekCount * 7);

  const weeks: CalendarWeek[] = [];
  for (let w = 0; w < weekCount; w++) {
    const days = eachDay(addDays(gridStart, w * 7), 7);
    weeks.push(layoutWeek(days, events));
  }

  return { weeks, gridStart, gridEndExclusive };
}

function layoutWeek(days: Date[], events: CalendarEvent[]): CalendarWeek {
  const weekStart = toISODate(days[0]);
  const weekEndExclusive = toISODate(addDays(days[0], 7));

  const segments = events
    .filter((e) => e.start < weekEndExclusive && weekStart < e.end)
    .map((e) => {
      const segStart = e.start > weekStart ? e.start : weekStart;
      const segEnd = e.end < weekEndExclusive ? e.end : weekEndExclusive;
      return {
        event: e,
        colStart: diffDays(weekStart, segStart),
        span: Math.max(1, diffDays(segStart, segEnd)),
        roundedLeft: e.start >= weekStart,
        roundedRight: e.end <= weekEndExclusive,
        lane: 0,
      } satisfies PlacedSegment;
    })
    .sort((a, b) => a.colStart - b.colStart || b.span - a.span || a.event.id.localeCompare(b.event.id));

  // Coloração gulosa de intervalos: como estão ordenados por início, a 1ª lane livre é ótima.
  const laneEnds: number[] = [];
  for (const seg of segments) {
    const end = seg.colStart + seg.span;
    let lane = laneEnds.findIndex((lastEnd) => lastEnd <= seg.colStart);
    if (lane === -1) {
      lane = laneEnds.length;
      laneEnds.push(end);
    } else {
      laneEnds[lane] = end;
    }
    seg.lane = lane;
  }

  return { days, segments, laneCount: laneEnds.length };
}

// Paleta categórica (cores padrão do Tailwind) — cor estável por acomodação.
const PALETTE = [
  "bg-sky-500/85 text-white",
  "bg-emerald-500/85 text-white",
  "bg-violet-500/85 text-white",
  "bg-amber-500/90 text-black",
  "bg-rose-500/85 text-white",
  "bg-teal-500/85 text-white",
  "bg-indigo-500/85 text-white",
  "bg-fuchsia-500/85 text-white",
];

export function accommodationColor(id: string): string {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) >>> 0;
  }
  return PALETTE[hash % PALETTE.length];
}
