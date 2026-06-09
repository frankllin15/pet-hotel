import { Loader2 } from "lucide-react";
import { cn } from "@/shared/lib/utils";

/** Único spinner do app — ninguém escreve o seu próprio (docs/08 §Estados padronizados). */
export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={cn("size-5 animate-spin text-muted-foreground", className)} aria-label="Carregando" />;
}
