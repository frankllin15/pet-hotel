import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { TabBar } from "@/shared/ui/tabs";
import { PetHealthPanel } from "@/features/health/components/pet-health-panel";
import { usePet } from "../queries";
import {
  BEHAVIOR_LEVEL_LABELS,
  BEHAVIOR_TRAITS,
  PET_SIZE_LABELS,
  SEX_LABELS,
  SPECIES_LABELS,
} from "../schemas";

const labelOf = (map: Record<string, string>, value: string | null | undefined) =>
  value ? (map[value] ?? value) : "—";

const neuteredLabel = (value: boolean | null | undefined) =>
  value === null || value === undefined ? "—" : value ? "Sim" : "Não";

type Tab = "general" | "health";

const TABS = [
  { value: "general" as const, label: "Geral" },
  { value: "health" as const, label: "Saúde" },
];

export function PetDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>("general");
  const query = usePet(id);

  return (
    <AsyncBoundary query={query} isEmpty={() => false}>
      {(pet) => (
        <DetailPage
          title={pet.name}
          description={SPECIES_LABELS[pet.species as keyof typeof SPECIES_LABELS] ?? pet.species}
          actions={
            <div className="flex gap-2">
              <Button onClick={() => navigate(`/registry/pets/${pet.id}/edit`)}>Editar</Button>
              <Button variant="outline" onClick={() => navigate(`/registry/tutors/${pet.tutorId}`)}>
                Ver tutor
              </Button>
              <Button variant="outline" onClick={() => navigate("/registry/pets")}>
                Voltar
              </Button>
            </div>
          }
          tabs={<TabBar items={TABS} value={tab} onChange={setTab} />}
          sidePanel={
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Dados</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <InfoRow label="Raça" value={pet.breed ?? "—"} />
                <InfoRow label="Nascimento" value={formatDate(pet.birthDate)} />
                <InfoRow label="Porte" value={labelOf(PET_SIZE_LABELS, pet.size)} />
                <InfoRow label="Sexo" value={labelOf(SEX_LABELS, pet.sex)} />
                <InfoRow label="Castrado" value={neuteredLabel(pet.neutered)} />
                <InfoRow label="Microchip" value={pet.microchipCode ?? "—"} />
                <InfoRow label="Cadastrado em" value={formatDate(pet.createdAt)} />
              </CardContent>
            </Card>
          }
        >
          {tab === "general" ? (
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Observações</CardTitle>
                </CardHeader>
                <CardContent className="text-sm text-muted-foreground">
                  {pet.notes ?? "Sem observações."}
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Avaliação comportamental</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {BEHAVIOR_TRAITS.map((trait) => (
                    <InfoRow
                      key={trait.key}
                      label={trait.label}
                      value={labelOf(BEHAVIOR_LEVEL_LABELS, pet[trait.key])}
                    />
                  ))}
                  {pet.behaviorNotes && (
                    <p className="border-t pt-2 text-muted-foreground">{pet.behaviorNotes}</p>
                  )}
                </CardContent>
              </Card>
            </div>
          ) : (
            <PetHealthPanel petId={pet.id} />
          )}
        </DetailPage>
      )}
    </AsyncBoundary>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4">
      <span className="text-muted-foreground">{label}</span>
      <span className="text-right font-medium">{value}</span>
    </div>
  );
}
