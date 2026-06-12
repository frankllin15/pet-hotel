import { useState } from "react";
import { Pencil, Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { PhotoThumb } from "@/shared/ui/photo-uploader";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import type { ParasiteTreatmentDto, VaccinationDto } from "../api";
import { usePetHealth, useVaccinationPhoto } from "../queries";
import { parasiteTreatmentLabel, vaccineLabel } from "../schemas";
import { ClearanceBadge, ParasiteStatusTag, VaccineStatusTag } from "./clearance-badge";
import { ParasiteTreatmentForm } from "./parasite-treatment-form";
import { VaccinationForm } from "./vaccination-form";
import { VetContactCard } from "./vet-contact-card";

/** Aba "Saúde" da ficha do pet: aptidão + vacinação + parasitas + veterinário. */
export function PetHealthPanel({ petId }: { petId: string }) {
  // null = fechado · "new" = registrar · DTO = editar aquele item.
  const [vaccForm, setVaccForm] = useState<VaccinationDto | "new" | null>(null);
  const [parasiteForm, setParasiteForm] = useState<ParasiteTreatmentDto | "new" | null>(null);
  const query = usePetHealth(petId);
  const vaccinePhoto = useVaccinationPhoto(petId);

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
                {vaccForm === null && (
                  <Button size="sm" onClick={() => setVaccForm("new")}>
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

              {vaccForm !== null && (
                <VaccinationForm
                  petId={petId}
                  vaccination={vaccForm === "new" ? undefined : vaccForm}
                  onDone={() => setVaccForm(null)}
                />
              )}

              {vaccinations.length > 0 ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Vacina</TableHead>
                      <TableHead>Aplicada em</TableHead>
                      <TableHead>Validade</TableHead>
                      <TableHead>Situação</TableHead>
                      <TableHead>Carteira</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
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
                        <TableCell>
                          <PhotoThumb
                            url={v.photoUrl ?? null}
                            onUpload={(file) =>
                              vaccinePhoto.upload.mutateAsync({ vaccinationId: v.id, file })
                            }
                            onRemove={() => vaccinePhoto.remove.mutateAsync(v.id)}
                            alt={`Carteira de ${vaccineLabel(v.type)}`}
                            confirmDescription={`A foto da carteira de ${vaccineLabel(v.type)} será removida permanentemente. Esta ação é irreversível.`}
                          />
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            size="sm"
                            variant="ghost"
                            aria-label="Editar vacina"
                            onClick={() => setVaccForm(v)}
                          >
                            <Pencil /> Editar
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                vaccForm === null && (
                  <p className="rounded-lg border border-dashed py-8 text-center text-sm text-muted-foreground">
                    Nenhuma vacina registrada.
                  </p>
                )
              )}

              <div className="flex flex-wrap items-center justify-between gap-3 pt-2">
                <h3 className="text-sm font-semibold">Controle de parasitas</h3>
                {parasiteForm === null && (
                  <Button size="sm" variant="outline" onClick={() => setParasiteForm("new")}>
                    <Plus /> Registrar controle
                  </Button>
                )}
              </div>

              {parasiteForm !== null && (
                <ParasiteTreatmentForm
                  petId={petId}
                  treatment={parasiteForm === "new" ? undefined : parasiteForm}
                  onDone={() => setParasiteForm(null)}
                />
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
                      <TableHead className="text-right">Ações</TableHead>
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
                        <TableCell className="text-right">
                          <Button
                            size="sm"
                            variant="ghost"
                            aria-label="Editar controle"
                            onClick={() => setParasiteForm(t)}
                          >
                            <Pencil /> Editar
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                parasiteForm === null && (
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
