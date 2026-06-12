import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { AuthImage } from "@/shared/ui/auth-image";
import { AvatarTile } from "@/shared/ui/avatar-tile";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { PhotoUploader } from "@/shared/ui/photo-uploader";
import { TabBar } from "@/shared/ui/tabs";
import { PetHealthPanel } from "@/features/health/components/pet-health-panel";
import { useDeletePet, usePet, usePetPhoto } from "../queries";
import {
  BEHAVIOR_LEVEL_LABELS,
  BEHAVIOR_TRAITS,
  FOOD_SOURCE_LABELS,
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
  const [confirmOpen, setConfirmOpen] = useState(false);
  const query = usePet(id);
  const photo = usePetPhoto(id);
  const deletePet = useDeletePet();

  async function runDelete() {
    try {
      await deletePet.mutateAsync(id);
      navigate("/registry/pets");
    } catch {
      // Falha é exibida no diálogo via deletePet.error.
    }
  }

  return (
    <AsyncBoundary query={query} isEmpty={() => false}>
      {(pet) => (
        <>
        <DetailPage
          title={
            <span className="flex items-center gap-3">
              {pet.photoUrl ? (
                <AuthImage
                  src={pet.photoUrl}
                  alt={pet.name}
                  className="size-12 rounded-xl object-cover"
                  fallback={<AvatarTile name={pet.name} size="lg" />}
                />
              ) : (
                <AvatarTile name={pet.name} size="lg" />
              )}
              {pet.name}
            </span>
          }
          description={SPECIES_LABELS[pet.species as keyof typeof SPECIES_LABELS] ?? pet.species}
          actions={
            <div className="flex gap-2">
              <Button onClick={() => navigate(`/registry/pets/${pet.id}/edit`)}>Editar</Button>
              <Button
                variant="destructive"
                onClick={() => {
                  deletePet.reset();
                  setConfirmOpen(true);
                }}
              >
                Excluir
              </Button>
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
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Foto</CardTitle>
                </CardHeader>
                <CardContent>
                  <PhotoUploader
                    url={pet.photoUrl ?? null}
                    onUpload={(file) => photo.upload.mutateAsync(file)}
                    onRemove={() => photo.remove.mutateAsync()}
                    alt={`Foto de ${pet.name}`}
                    confirmDescription={`A foto de ${pet.name} será removida permanentemente. Esta ação é irreversível.`}
                    fallback={<AvatarTile name={pet.name} size="lg" className="size-full rounded-xl text-4xl" />}
                  />
                </CardContent>
              </Card>
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
            </div>
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
                  <CardTitle className="text-sm">Rotina alimentar</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {pet.feedingRoutine ? (
                    <>
                      <InfoRow label="Ração" value={pet.feedingRoutine.foodName} />
                      <InfoRow label="Quantidade" value={pet.feedingRoutine.portionSize ?? "—"} />
                      <InfoRow
                        label="Horários"
                        value={
                          pet.feedingRoutine.mealTimes.length > 0
                            ? pet.feedingRoutine.mealTimes.map((t) => t.slice(0, 5)).join(" · ")
                            : "—"
                        }
                      />
                      <InfoRow label="Origem" value={labelOf(FOOD_SOURCE_LABELS, pet.feedingRoutine.foodSource)} />
                      {pet.feedingRoutine.restrictions && (
                        <p className="border-t pt-2 text-muted-foreground">{pet.feedingRoutine.restrictions}</p>
                      )}
                    </>
                  ) : (
                    <p className="text-muted-foreground">Sem rotina alimentar cadastrada.</p>
                  )}
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Pertences trazidos</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {pet.belongings.length > 0 ? (
                    pet.belongings.map((b, i) => (
                      <InfoRow
                        key={i}
                        label={b.notes ? `${b.name} — ${b.notes}` : b.name}
                        value={`${b.quantity}×`}
                      />
                    ))
                  ) : (
                    <p className="text-muted-foreground">Nenhum pertence registrado.</p>
                  )}
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
        <ConfirmDialog
          open={confirmOpen}
          loading={deletePet.isPending}
          title="Excluir pet"
          description={
            <p>
              O pet <strong>{pet.name}</strong> e a foto associada serão excluídos permanentemente.
              Esta ação é irreversível.
            </p>
          }
          confirmLabel="Excluir"
          confirmVariant="destructive"
          onConfirm={() => void runDelete()}
          onClose={() => setConfirmOpen(false)}
        />
        </>
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
