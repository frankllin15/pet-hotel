import { useState } from "react";
import { Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { usePetHealth } from "../queries";
import { vaccineLabel } from "../schemas";
import { ClearanceBadge, VaccineStatusTag } from "./clearance-badge";
import { VaccinationForm } from "./vaccination-form";

/** Aba "Saúde" da ficha do pet: aptidão + carteira de vacinação + registro. */
export function PetHealthPanel({ petId }: { petId: string }) {
  const [showForm, setShowForm] = useState(false);
  const query = usePetHealth(petId);

  return (
    <div className="space-y-4">
      <AsyncBoundary query={query} isEmpty={() => false}>
        {(health) => {
          const pendencies = health?.pendencies ?? [];
          const vaccinations = health?.vaccinations ?? [];
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
            </div>
          );
        }}
      </AsyncBoundary>
    </div>
  );
}
