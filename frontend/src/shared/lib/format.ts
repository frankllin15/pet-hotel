const dateFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium" });
const dateTimeFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium", timeStyle: "short" });

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
