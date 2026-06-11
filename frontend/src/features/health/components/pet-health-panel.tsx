import { useState } from "react";
import { Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { usePetHealth } from "../queries";
import { parasiteTreatmentLabel, vaccineLabel } from "../schemas";
import { ClearanceBadge, ParasiteStatusTag, VaccineStatusTag } from "./clearance-badge";
import { ParasiteTreatmentForm } from "./parasite-treatment-form";
import { VaccinationForm } from "./vaccination-form";
import { VetContactCard } from "./vet-contact-card";

/** Aba "Saúde" da ficha do pet: aptidão + vacinação + parasitas + veterinário. */
export function PetHealthPanel({ petId }: { petId: string }) {
  const [showForm, setShowForm] = useState(false);
  const [showParasiteForm, setShowParasiteForm] = useState(false);
  const query = usePetHealth(petId);

  return (
    <div className="space-y-4">
      <AsyncBoundary query={query} isEmpty={() => false}>
        {(health) => {
          const pendencies = health?.pendencies ?? [];
          const vaccinations = health?.vaccinations ?? [];
          const parasiteTreatments = health?.parasiteTreatments ?? [];
          return (
            <div className="space-y-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                  {health ? (
                    <ClearanceBadge isCleared={health.isCleared} />
                  ) : (
                    <span className="text-sm text-muted-foreground">Sem ficha de saúde ainda.</span>
                  )}
                </div>
                {!showForm && (
                  <Button size="sm" onClick={() => setShowForm(true)}>
                    <Plus /> Registrar vacina
                  </Button>
                )}
              </div>

              {pendencies.length > 0 && (
                <p className="rounded-md border border-warning/40 bg-warning/10 px-3 py-2 text-sm">
                  Vacinas obrigatórias pendentes ou vencidas:{" "}
                  <span className="font-medium">{pendencies.map(vaccineLabel).join(", ")}</span>.
                </p>
              )}

              {showForm && <VaccinationForm petId={petId} onDone={() => setShowForm(false)} />}

              {vaccinations.length > 0 ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Vacina</TableHead>
                      <TableHead>Aplicada em</TableHead>
                      <TableHead>Validade</TableHead>
                      <TableHead>Situação</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {vaccinations.map((v) => (
                      <TableRow key={v.id}>
                        <TableCell className="font-medium">{vaccineLabel(v.type)}</TableCell>
                        <TableCell>{formatDate(v.appliedOn)}</TableCell>
                        <TableCell>{formatDate(v.expiresOn)}</TableCell>
                        <TableCell>
                          <VaccineStatusTag valid={v.valid} />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                !showForm && (
                  <p className="rounded-lg border border-dashed py-8 text-center text-sm text-muted-foreground">
                    Nenhuma vacina registrada.
                  </p>
                )
              )}

              <div className="flex flex-wrap items-center justify-between gap-3 pt-2">
                <h3 className="text-sm font-semibold">Controle de parasitas</h3>
                {!showParasiteForm && (
                  <Button size="sm" variant="outline" onClick={() => setShowParasiteForm(true)}>
                    <Plus /> Registrar controle
                  </Button>
                )}
              </div>

              {showParasiteForm && (
                <ParasiteTreatmentForm petId={petId} onDone={() => setShowParasiteForm(false)} />
              )}

              {parasiteTreatments.length > 0 ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Tipo</TableHead>
                      <TableHead>Produto</TableHead>
                      <TableHead>Aplicado em</TableHead>
                      <TableHead>Próxima dose</TableHead>
                      <TableHead>Situação</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {parasiteTreatments.map((t) => (
                      <TableRow key={t.id}>
                        <TableCell className="font-medium">{parasiteTreatmentLabel(t.type)}</TableCell>
                        <TableCell>{t.productName ?? "—"}</TableCell>
                        <TableCell>{formatDate(t.appliedOn)}</TableCell>
                        <TableCell>{t.nextDueOn ? formatDate(t.nextDueOn) : "—"}</TableCell>
                        <TableCell>
                          <ParasiteStatusTag upToDate={t.upToDate} />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                !showParasiteForm && (
                  <p className="rounded-lg border border-dashed py-8 text-center text-sm text-muted-foreground">
                    Nenhum controle de parasitas registrado.
                  </p>
                )
              )}

              <VetContactCard petId={petId} vetContact={health?.vetContact ?? null} />
            </div>
          );
        }}
      </AsyncBoundary>
    </div>
  );
}
