import { useMemo, useState } from "react";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import type { ConsentDto } from "../api";
import { useSetTutorConsents } from "../queries";
import { CONSENT_TYPES, CONSENT_TYPE_DESCRIPTIONS, CONSENT_TYPE_LABELS } from "../schemas";

type ConsentTypeName = (typeof CONSENT_TYPES)[number];

/**
 * Painel de consentimentos LGPD do tutor: um toggle por finalidade, com a data da decisão.
 * Envia todas as decisões ao salvar; o backend preserva o carimbo das inalteradas.
 */
export function TutorConsentsCard({ tutorId, consents }: { tutorId: string; consents: ConsentDto[] }) {
  const mutation = useSetTutorConsents(tutorId);

  // Estado vigente no servidor por finalidade (default: não concedido).
  const baseline = useMemo(
    () =>
      Object.fromEntries(
        CONSENT_TYPES.map((t) => [t, consents.find((c) => c.type === t)?.granted ?? false]),
      ) as Record<ConsentTypeName, boolean>,
    [consents],
  );
  const [draft, setDraft] = useState(baseline);
  // Re-sincroniza o rascunho quando os dados do servidor mudam (ex.: após salvar) —
  // padrão "ajustar estado na render" do React (sem efeito, sem renders em cascata).
  const [syncedBaseline, setSyncedBaseline] = useState(baseline);
  if (syncedBaseline !== baseline) {
    setSyncedBaseline(baseline);
    setDraft(baseline);
  }

  const dirty = CONSENT_TYPES.some((t) => draft[t] !== baseline[t]);
  const decidedAt = (type: ConsentTypeName) => consents.find((c) => c.type === type)?.decidedAt;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm">Consentimentos (LGPD)</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {CONSENT_TYPES.map((type) => {
          const at = decidedAt(type);
          return (
            <label
              key={type}
              className="flex cursor-pointer items-start justify-between gap-4 rounded-lg border border-border bg-card px-3 py-2.5 hover:bg-accent/40"
            >
              <span className="text-sm">
                <span className="font-medium">{CONSENT_TYPE_LABELS[type]}</span>
                <span className="block text-xs text-muted-foreground">{CONSENT_TYPE_DESCRIPTIONS[type]}</span>
                {at && (
                  <span className="mt-0.5 block text-xs text-muted-foreground">
                    {baseline[type] ? "Concedido" : "Negado"} em {formatDate(at)}
                  </span>
                )}
              </span>
              <input
                type="checkbox"
                className="mt-0.5 size-4 shrink-0 accent-primary"
                checked={draft[type]}
                onChange={(e) => setDraft({ ...draft, [type]: e.target.checked })}
              />
            </label>
          );
        })}

        {mutation.error instanceof ApiError && (
          <p className="text-sm text-destructive">{mutation.error.message}</p>
        )}

        <div className="flex justify-end">
          <Button
            size="sm"
            disabled={!dirty || mutation.isPending}
            onClick={() => mutation.mutate(CONSENT_TYPES.map((t) => ({ type: t, granted: draft[t] })))}
          >
            {mutation.isPending ? "Salvando…" : "Salvar consentimentos"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
