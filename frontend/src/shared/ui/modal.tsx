import { type ReactNode, useEffect } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";

export interface ModalProps {
  open: boolean;
  /** Título do diálogo. */
  title: string;
  /** Subtítulo opcional (contexto curto). */
  description?: ReactNode;
  /** Largura máxima via classe Tailwind (default max-w-md). */
  maxWidth?: string;
  /** Bloqueia o fechar por overlay/Esc/X enquanto uma ação roda. */
  busy?: boolean;
  onClose: () => void;
  children: ReactNode;
}

/**
 * Diálogo modal genérico (sem Radix, mesmo padrão do <ConfirmDialog>): portal para o body,
 * overlay, fechar por Esc/overlay/X. Use para formulários curtos de criação/edição.
 */
export function Modal({
  open,
  title,
  description,
  maxWidth = "max-w-md",
  busy = false,
  onClose,
  children,
}: ModalProps) {
  // Esc fecha (a menos que uma ação esteja em andamento).
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !busy) onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, busy, onClose]);

  if (!open) return null;

  // Portal para o body: escapa ancestrais com `transform`/`filter` (ex.: a animação .rise-in),
  // que senão tornariam o `position: fixed` relativo a eles e encolheriam o overlay.
  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      role="dialog"
      aria-modal="true"
      aria-label={title}
      onClick={() => !busy && onClose()}
    >
      <div
        className={`w-full ${maxWidth} rounded-lg border bg-background p-6 shadow-lg`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="font-display text-lg font-semibold">{title}</h2>
            {description && <p className="mt-1 text-sm text-muted-foreground">{description}</p>}
          </div>
          <button
            type="button"
            onClick={() => !busy && onClose()}
            disabled={busy}
            className="-mr-1 -mt-1 rounded-md p-1 text-muted-foreground hover:bg-muted disabled:opacity-50"
            aria-label="Fechar"
          >
            <X className="size-4" />
          </button>
        </div>
        <div className="mt-4">{children}</div>
      </div>
    </div>,
    document.body,
  );
}
