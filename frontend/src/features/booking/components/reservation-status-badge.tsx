import { Badge } from "@/shared/ui/badge";
import { RESERVATION_STATUS_LABELS } from "../schemas";

/** Status da reserva — componente de domínio (docs/08 §Componentes). */
const STATUS_VARIANTS: Record<string, "success" | "secondary" | "warning" | "default"> = {
  Requested: "warning",
  Confirmed: "success",
  CheckedIn: "default",
  CheckedOut: "secondary",
  Cancelled: "secondary",
};

export function ReservationStatusBadge({ status }: { status: string }) {
  return (
    <Badge variant={STATUS_VARIANTS[status] ?? "warning"}>
      {RESERVATION_STATUS_LABELS[status] ?? status}
    </Badge>
  );
}
