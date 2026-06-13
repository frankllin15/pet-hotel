const dateFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium" });
const moneyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

/** Formata um valor monetário em BRL. Aceita number ou string (o contrato traz decimais como number|string). */
export function formatMoney(value: number | string | null | undefined): string {
  if (value === null || value === undefined || value === "") return "—";
  const amount = Number(value);
  return Number.isNaN(amount) ? "—" : moneyFormatter.format(amount);
}
const dateTimeFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium", timeStyle: "short" });
const timeFormatter = new Intl.DateTimeFormat("pt-BR", { hour: "2-digit", minute: "2-digit" });

/** Formata só a hora (HH:mm) de um instante ISO em pt-BR; vazio para nulo/inválido. */
export function formatTime(iso: string | null | undefined): string {
  if (!iso) return "—";
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "—" : timeFormatter.format(date);
}

/** Formata uma data ISO (date ou date-time) em pt-BR; vazio para nulo/ inválido. */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return "—";
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "—" : dateFormatter.format(date);
}

/** Formata um instante ISO (date-time) em pt-BR com data e hora; vazio para nulo/inválido. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return "—";
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "—" : dateTimeFormatter.format(date);
}
