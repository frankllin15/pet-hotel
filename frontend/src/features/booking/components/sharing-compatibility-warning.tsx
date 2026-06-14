import { AlertTriangle } from "lucide-react";
import type { SharingCompatibilityDto } from "../api";
import { COMPATIBILITY_FLAG_LABELS } from "../schemas";

/**
 * Aviso (não-bloqueante) de compatibilidade ao dividir a acomodação: lista os pets do grupo
 * com sinais comportamentais de atenção. Não renderiza nada quando não há conflito.
 */
export function SharingCompatibilityWarning({ data }: { data: SharingCompatibilityDto | undefined }) {
  if (!data?.shared || data.conflicts.length === 0) return null;

  return (
    <div className="flex gap-3 rounded-lg border border-warning/40 bg-warning/10 px-4 py-3 text-sm">
      <AlertTriangle className="mt-0.5 size-4 shrink-0 text-warning" />
      <div className="space-y-1">
        <p className="font-medium">Atenção ao compartilhar a acomodação</p>
        <ul className="space-y-0.5 text-muted-foreground">
          {data.conflicts.map((pet) => (
            <li key={pet.petId}>
              <span className="font-medium text-foreground">{pet.name}</span>
              {" — "}
              {pet.flags.map((f) => COMPATIBILITY_FLAG_LABELS[f] ?? f).join(", ")}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
