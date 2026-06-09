/** Utilitários de data trabalhando com ISO "yyyy-mm-dd" no fuso local. */

export function toISODate(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
}

export function addDays(date: Date, days: number): Date {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return next;
}

export function startOfToday(): Date {
  const now = new Date();
  return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}

/** Lista `count` datas consecutivas a partir de `start`. */
export function eachDay(start: Date, count: number): Date[] {
  return Array.from({ length: count }, (_, i) => addDays(start, i));
}
