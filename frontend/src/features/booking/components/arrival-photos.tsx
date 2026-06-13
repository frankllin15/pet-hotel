import { useRef, type ChangeEvent } from "react";
import { Plus, Trash2 } from "lucide-react";
import { ACCEPTED_IMAGE_TYPES } from "@/shared/api/files";
import { ApiError } from "@/shared/lib/problem-details";
import { AuthImage } from "@/shared/ui/auth-image";
import { Button } from "@/shared/ui/button";
import { useArrivalPhotos } from "../queries";

/** A URL de download é `/v1/files/{key}`; a remoção precisa da chave. */
const keyFromUrl = (url: string) => url.replace(/^\/v1\/files\//, "");

/**
 * Galeria de fotos de chegada da reserva. Upload/remoção só quando o pet já chegou
 * (canManage = reserva em estadia/encerrada); caso contrário exibe só as fotos existentes.
 */
export function ArrivalPhotos({
  reservationId,
  photoUrls,
  canManage,
}: {
  reservationId: string;
  photoUrls: string[];
  canManage: boolean;
}) {
  const { upload, remove } = useArrivalPhotos(reservationId);
  const inputRef = useRef<HTMLInputElement>(null);

  const mutationError = upload.error ?? remove.error;
  const error = mutationError instanceof ApiError ? mutationError.message : null;

  const onPick = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) upload.mutate(file);
    e.target.value = "";
  };

  return (
    <div className="space-y-3">
      {photoUrls.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          {canManage ? "Nenhuma foto de chegada." : "O pet ainda não chegou — fotos disponíveis após o check-in."}
        </p>
      ) : (
        <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
          {photoUrls.map((url) => (
            <div key={url} className="group relative aspect-square overflow-hidden rounded-lg border bg-muted">
              <AuthImage
                src={url}
                alt="Foto de chegada"
                className="size-full object-cover"
                fallback={<div className="flex size-full items-center justify-center text-xs text-muted-foreground">…</div>}
              />
              {canManage && (
                <button
                  type="button"
                  aria-label="Remover foto"
                  className="absolute right-1 top-1 rounded-md bg-background/80 p-1 text-destructive opacity-0 transition group-hover:opacity-100"
                  onClick={() => remove.mutate(keyFromUrl(url))}
                  disabled={remove.isPending}
                >
                  <Trash2 className="size-4" />
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {canManage && (
        <div>
          <input
            ref={inputRef}
            type="file"
            accept={ACCEPTED_IMAGE_TYPES.join(",")}
            className="hidden"
            onChange={onPick}
          />
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={upload.isPending}
            onClick={() => inputRef.current?.click()}
          >
            <Plus /> {upload.isPending ? "Enviando…" : "Adicionar foto"}
          </Button>
        </div>
      )}

      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
