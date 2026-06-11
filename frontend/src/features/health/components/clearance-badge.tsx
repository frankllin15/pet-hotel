import { CheckCircle2, ShieldAlert } from "lucide-react";
import { Badge } from "@/shared/ui/badge";

/** Aptidão sanitária do pet (clearance) — componente de domínio (docs/08). */
export function ClearanceBadge({ isCleared }: { isCleared: boolean }) {
  return isCleared ? (
    <Badge variant="success" className="gap-1">
      <CheckCircle2 className="size-3.5" /> Apto
    </Badge>
  ) : (
    <Badge variant="warning" className="gap-1">
      <ShieldAlert className="size-3.5" /> Pendência sanitária
    </Badge>
  );
}

/** Situação de uma vacina (em dia / vencida). */
export function VaccineStatusTag({ valid }: { valid: boolean }) {
  return valid ? (
    <Badge variant="success">Em dia</Badge>
  ) : (
    <Badge variant="destructive">Vencida</Badge>
  );
}

/** Situação de um controle de parasitas (em dia / vencido / sem próxima dose informada). */
export function ParasiteStatusTag({ upToDate }: { upToDate: boolean | null }) {
  if (upToDate === null) {
    return <Badge variant="outline">Sem previsão</Badge>;
  }
  return upToDate ? <Badge variant="success">Em dia</Badge> : <Badge variant="destructive">Vencido</Badge>;
}
