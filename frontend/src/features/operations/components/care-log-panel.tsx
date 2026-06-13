import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ImagePlus, Plus, Trash2 } from "lucide-react";
import { ACCEPTED_IMAGE_TYPES } from "@/shared/api/files";
import { formatDateTime } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { AuthImage } from "@/shared/ui/auth-image";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { Field } from "@/shared/ui/field";
import { Select } from "@/shared/ui/select";
import { Textarea } from "@/shared/ui/textarea";
import { UserName } from "@/features/users/components/user-name";
import { useCareEntryPhotos, useLogCareEntry, useStayCareLog } from "../queries";
import { CARE_LOG_TYPE_LABELS, CARE_LOG_TYPES, careLogFormSchema, type CareLogFormInput } from "../schemas";

const keyFromUrl = (url: string) => url.replace(/^\/v1\/files\//, "");

/**
 * Diário de bordo de uma estadia: registro rápido de ocorrência + timeline. O registro só
 * fica disponível quando o pet chegou (canManage); a timeline é sempre visível.
 */
export function CareLogPanel({ reservationId, canManage }: { reservationId: string; canManage: boolean }) {
  const query = useStayCareLog(reservationId);
  const logEntry = useLogCareEntry(reservationId);
  const photos = useCareEntryPhotos(reservationId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CareLogFormInput>({
    resolver: zodResolver(careLogFormSchema),
    defaultValues: { type: "Meal", note: "" },
  });

  const submit = handleSubmit((values) =>
    logEntry.mutate(
      { type: values.type, note: values.note ? values.note : null },
      { onSuccess: () => reset({ type: values.type, note: "" }) },
    ),
  );

  const formError = logEntry.error instanceof ApiError ? logEntry.error.message : null;

  return (
    <div className="space-y-6">
      {canManage ? (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Registrar ocorrência</CardTitle>
          </CardHeader>
          <CardContent>
            <form className="flex flex-wrap items-end gap-3" onSubmit={submit}>
              <div className="w-44">
                <Field label="Tipo" htmlFor="care-type" error={errors.type?.message}>
                  <Select id="care-type" {...register("type")}>
                    {CARE_LOG_TYPES.map((t) => (
                      <option key={t} value={t}>
                        {CARE_LOG_TYPE_LABELS[t]}
                      </option>
                    ))}
                  </Select>
                </Field>
              </div>
              <div className="min-w-48 flex-1">
                <Field label="Observação" htmlFor="care-note" error={errors.note?.message}>
                  <Textarea id="care-note" rows={1} placeholder="opcional" {...register("note")} />
                </Field>
              </div>
              <Button type="submit" disabled={logEntry.isPending}>
                <Plus /> {logEntry.isPending ? "Registrando…" : "Registrar"}
              </Button>
            </form>
            {formError && <p className="mt-2 text-sm text-destructive">{formError}</p>}
          </CardContent>
        </Card>
      ) : (
        <p className="rounded-lg border border-dashed py-4 text-center text-sm text-muted-foreground">
          O diário fica disponível após o check-in do pet.
        </p>
      )}

      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.pages.every((p) => p.items.length === 0)}
        empty={
          <p className="rounded-lg border border-dashed py-8 text-center text-sm text-muted-foreground">
            Nenhuma ocorrência registrada ainda.
          </p>
        }
      >
        {(data) => {
          const entries = data.pages.flatMap((p) => p.items);
          return (
            <div className="space-y-3">
              <ol className="space-y-4 border-l border-border pl-4">
                {entries.map((e) => (
                  <li key={e.id} className="relative">
                    <span className="absolute -left-[1.3125rem] top-1.5 size-2 rounded-full bg-primary" />
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant="secondary">{CARE_LOG_TYPE_LABELS[e.type] ?? e.type}</Badge>
                      <span className="text-xs text-muted-foreground">{formatDateTime(e.occurredAt)}</span>
                      {e.registeredBy && (
                        <span className="text-xs text-muted-foreground">
                          · <UserName userId={e.registeredBy} />
                        </span>
                      )}
                    </div>
                    {e.note && <p className="mt-1 text-sm">{e.note}</p>}
                    {(e.photoUrls.length > 0 || canManage) && (
                      <div className="mt-2 flex flex-wrap items-center gap-2">
                        {e.photoUrls.map((url) => (
                          <div key={url} className="group relative size-16 overflow-hidden rounded-md border bg-muted">
                            <AuthImage src={url} alt="Foto" className="size-full object-cover" />
                            {canManage && (
                              <button
                                type="button"
                                aria-label="Remover foto"
                                className="absolute right-0.5 top-0.5 rounded bg-background/80 p-0.5 text-destructive opacity-0 transition group-hover:opacity-100"
                                onClick={() => photos.remove.mutate({ entryId: e.id, key: keyFromUrl(url) })}
                              >
                                <Trash2 className="size-3.5" />
                              </button>
                            )}
                          </div>
                        ))}
                        {canManage && (
                          <label className="flex size-16 cursor-pointer items-center justify-center rounded-md border border-dashed text-muted-foreground hover:bg-accent/40">
                            <input
                              type="file"
                              accept={ACCEPTED_IMAGE_TYPES.join(",")}
                              className="hidden"
                              onChange={(ev) => {
                                const file = ev.target.files?.[0];
                                if (file) photos.upload.mutate({ entryId: e.id, file });
                                ev.target.value = "";
                              }}
                            />
                            <ImagePlus className="size-4" />
                          </label>
                        )}
                      </div>
                    )}
                  </li>
                ))}
              </ol>
              {query.hasNextPage && (
                <div className="flex justify-center">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => query.fetchNextPage()}
                    disabled={query.isFetchingNextPage}
                  >
                    {query.isFetchingNextPage ? "Carregando…" : "Carregar mais"}
                  </Button>
                </div>
              )}
            </div>
          );
        }}
      </AsyncBoundary>
    </div>
  );
}
