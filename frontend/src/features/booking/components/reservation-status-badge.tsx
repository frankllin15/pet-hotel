import { Badge } from "@/shared/ui/badge";
import { RESERVATION_STATUS_LABELS } from "../schemas";

/** Status da reserva — componente de domínio (docs/08 §Componentes). */
export function ReservationStatusBadge({ status }: { status: string }) {
  const variant =
    status === "Confirmed" ? "success" : status === "Cancelled" ? "secondary" : "warning";
  return <Badge variant={variant}>{RESERVATION_STATUS_LABELS[status] ?? status}</Badge>;
}
