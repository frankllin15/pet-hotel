import { type ReactNode, useEffect } from "react";
import { createPortal } from "react-dom";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/ui/button";

export interface ConfirmDialogProps {
  open: boolean;
  /** Título curto da ação (ex.: "Confirmar check-in"). */
  title: string;
  /** Corpo didático: explica o que acontece e as consequências. */
  description: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  /** Visual do botão de confirmação — use "destructive" para ações irreversíveis/perigosas. */
  confirmVariant?: "default" | "destructive";
  /** Bloqueia os botões e mostra spinner enquanto a ação roda. */
  loading?: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/**
 * Diálogo de confirmação controlado (sem Radix, padrão dos primitivos — ver TabBar).
 * Para ações com consequência no balcão: pede confirmação e explica o efeito.
 */
export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = "Confirmar",
  cancelLabel = "Voltar",
  confirmVariant = "default",
  loading = false,
  onConfirm,
  onClose,
}: ConfirmDialogProps) {
  // Esc fecha o diálogo (a menos que uma ação esteja em andamento).
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !loading) onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, loading, onClose]);

  if (!open) return null;

  // Portal para o body: escapa ancestrais com `transform`/`filter` (ex.: a animação
  // .rise-in das telas), que senão tornariam o `position: fixed` relativo a eles e
  // encolheriam o overlay para menor que a viewport.
  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      role="dialog"
      aria-modal="true"
      aria-label={title}
      onClick={() => !loading && onClose()}
    >
      <div
        className="w-full max-w-md rounded-lg border bg-background p-6 shadow-lg"
        onClick={(e) => e.stopPropagation()}
      >
        <h2 className="text-lg font-semibold">{title}</h2>
        <div className="mt-3 space-y-2 text-sm text-muted-foreground">{description}</div>
        <div className="mt-6 flex justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={loading}>
            {cancelLabel}
          </Button>
          <Button variant={confirmVariant} onClick={onConfirm} disabled={loading}>
            {loading && <Loader2 className="size-4 animate-spin" />}
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
