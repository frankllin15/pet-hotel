const dateFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium" });

/** Formata uma data ISO (date ou date-time) em pt-BR; vazio para nulo/ inválido. */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return "—";
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "—" : dateFormatter.format(date);
}
