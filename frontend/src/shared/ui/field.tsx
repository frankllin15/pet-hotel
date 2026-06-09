import { type ReactNode } from "react";
import { Label } from "./label";

/** Linha de formulário padrão: rótulo + controle + mensagem de erro (docs/08 §Form). */
export function Field({
  label,
  htmlFor,
  error,
  hint,
  children,
}: {
  label: ReactNode;
  htmlFor?: string;
  error?: string;
  hint?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      {hint && !error && <p className="text-xs text-muted-foreground">{hint}</p>}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}
