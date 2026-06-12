import { useRef, useState, type ReactNode } from "react";
import { Camera, Loader2, Trash2, Upload, X } from "lucide-react";
import { ACCEPTED_IMAGE_TYPES, MAX_IMAGE_BYTES } from "@/shared/api/files";
import { ApiError } from "@/shared/lib/problem-details";
import { cn } from "@/shared/lib/utils";
import { AuthImage } from "./auth-image";
import { Button } from "./button";
import { ConfirmDialog } from "./confirm-dialog";

/** Validação client-side (UX); o backend é a fonte da verdade. Mensagens em pt-BR. */
function validateImage(file: File): string | null {
  if (!ACCEPTED_IMAGE_TYPES.includes(file.type as (typeof ACCEPTED_IMAGE_TYPES)[number])) {
    return "Formato inválido. Use JPEG, PNG ou WebP.";
  }
  if (file.size > MAX_IMAGE_BYTES) {
    return `Arquivo acima do limite de ${MAX_IMAGE_BYTES / (1024 * 1024)} MB.`;
  }
  return null;
}

function messageOf(error: unknown): string {
  return error instanceof ApiError ? error.message : "Não foi possível enviar a imagem.";
}

interface BaseProps {
  /** URL relativa da foto atual (ex.: /v1/files/...), ou null quando não há. */
  url: string | null;
  onUpload: (file: File) => Promise<unknown>;
  onRemove: () => Promise<unknown>;
  alt: string;
  /** Corpo do diálogo de confirmação de remoção (default genérico). */
  confirmDescription?: ReactNode;
}

const DEFAULT_REMOVE_DESCRIPTION =
  "A imagem será removida permanentemente. Esta ação é irreversível.";

/** Uploader em destaque (ficha do pet): moldura quadrada + ações abaixo. */
export function PhotoUploader({
  url,
  onUpload,
  onRemove,
  alt,
  fallback,
  confirmDescription = DEFAULT_REMOVE_DESCRIPTION,
}: BaseProps & { fallback?: ReactNode }) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);

  async function run(action: () => Promise<unknown>) {
    setBusy(true);
    setError(null);
    try {
      await action();
    } catch (err) {
      setError(messageOf(err));
    } finally {
      setBusy(false);
    }
  }

  async function confirmRemove() {
    await run(onRemove);
    setConfirmOpen(false);
  }

  function onPick(file: File | undefined) {
    if (!file) return;
    const invalid = validateImage(file);
    if (invalid) {
      setError(invalid);
      return;
    }
    void run(() => onUpload(file));
  }

  return (
    <div className="space-y-3">
      <div className="relative mx-auto aspect-square w-40 overflow-hidden rounded-xl border bg-muted">
        {url ? (
          <AuthImage src={url} alt={alt} className="size-full object-cover" fallback={fallback} />
        ) : (
          <div className="flex size-full items-center justify-center">{fallback}</div>
        )}
        {busy && (
          <div className="absolute inset-0 grid place-items-center bg-background/60">
            <Loader2 className="size-6 animate-spin text-muted-foreground" />
          </div>
        )}
      </div>

      <div className="flex justify-center gap-2">
        <Button size="sm" variant="outline" disabled={busy} onClick={() => inputRef.current?.click()}>
          <Upload /> {url ? "Trocar" : "Enviar foto"}
        </Button>
        {url && (
          <Button size="sm" variant="ghost" disabled={busy} onClick={() => setConfirmOpen(true)}>
            <Trash2 /> Remover
          </Button>
        )}
      </div>

      {error && <p className="text-center text-xs text-destructive">{error}</p>}

      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_IMAGE_TYPES.join(",")}
        className="hidden"
        onChange={(e) => {
          onPick(e.target.files?.[0]);
          e.target.value = ""; // permite reenviar o mesmo arquivo
        }}
      />

      <ConfirmDialog
        open={confirmOpen}
        loading={busy}
        title="Remover foto"
        description={confirmDescription}
        confirmLabel="Remover"
        confirmVariant="destructive"
        onConfirm={() => void confirmRemove()}
        onClose={() => setConfirmOpen(false)}
      />
    </div>
  );
}

/** Versão compacta para linhas de tabela (carteira de vacina): miniatura clicável. */
export function PhotoThumb({
  url,
  onUpload,
  onRemove,
  alt,
  confirmDescription = DEFAULT_REMOVE_DESCRIPTION,
}: BaseProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);

  async function run(action: () => Promise<unknown>) {
    setBusy(true);
    try {
      await action();
    } finally {
      setBusy(false);
    }
  }

  async function confirmRemove() {
    await run(onRemove);
    setConfirmOpen(false);
  }

  function onPick(file: File | undefined) {
    if (!file || validateImage(file)) return;
    void run(() => onUpload(file));
  }

  return (
    <div className="flex items-center gap-1">
      <button
        type="button"
        disabled={busy}
        title={url ? "Trocar foto da carteira" : "Enviar foto da carteira"}
        onClick={() => inputRef.current?.click()}
        className={cn(
          "grid size-10 place-items-center overflow-hidden rounded-md border bg-muted text-muted-foreground",
          "transition-colors hover:border-primary hover:text-primary disabled:opacity-60",
          !url && "border-dashed",
        )}
      >
        {busy ? (
          <Loader2 className="size-4 animate-spin" />
        ) : url ? (
          <AuthImage
            src={url}
            alt={alt}
            className="size-full object-cover"
            fallback={<Camera className="size-4" />}
          />
        ) : (
          <Camera className="size-4" />
        )}
      </button>
      {url && !busy && (
        <button
          type="button"
          title="Remover foto"
          onClick={() => setConfirmOpen(true)}
          className="text-muted-foreground transition-colors hover:text-destructive"
        >
          <X className="size-4" />
        </button>
      )}
      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_IMAGE_TYPES.join(",")}
        className="hidden"
        onChange={(e) => {
          onPick(e.target.files?.[0]);
          e.target.value = "";
        }}
      />

      <ConfirmDialog
        open={confirmOpen}
        loading={busy}
        title="Remover foto"
        description={confirmDescription}
        confirmLabel="Remover"
        confirmVariant="destructive"
        onConfirm={() => void confirmRemove()}
        onClose={() => setConfirmOpen(false)}
      />
    </div>
  );
}
