import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Plus } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { AvatarTile } from "@/shared/ui/avatar-tile";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { TutorReportsCard } from "@/features/notifications/components/tutor-reports-card";
import { TutorConsentsCard } from "../components/tutor-consents-card";
import { SPECIES_LABELS } from "../schemas";
import { useDeleteTutor, usePets, useTutor } from "../queries";

export function TutorDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const tutorQuery = useTutor(id);
  const petsQuery = usePets({ tutorId: id });
  const deleteTutor = useDeleteTutor();
  const [confirmOpen, setConfirmOpen] = useState(false);

  async function runDelete() {
    try {
      await deleteTutor.mutateAsync(id);
      navigate("/registry/tutors");
    } catch {
      // Falha (ex.: 409 com pets vinculados) é exibida no diálogo via deleteTutor.error.
    }
  }

  return (
    <AsyncBoundary query={tutorQuery} isEmpty={() => false}>
      {(tutor) => (
        <>
        <DetailPage
          title={
            <span className="flex items-center gap-3">
              <AvatarTile name={tutor.fullName} size="lg" />
              {tutor.fullName}
            </span>
          }
          description="Ficha do tutor"
          actions={
            <div className="flex gap-2">
              <Button onClick={() => navigate(`/registry/tutors/${tutor.id}/edit`)}>Editar</Button>
              <Button
                variant="destructive"
                onClick={() => {
                  deleteTutor.reset();
                  setConfirmOpen(true);
                }}
              >
                Excluir
              </Button>
              <Button variant="outline" onClick={() => navigate("/registry/tutors")}>
                Voltar
              </Button>
            </div>
          }
          sidePanel={
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Contato</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  <InfoRow label="E-mail" value={tutor.email} />
                  <InfoRow label="Telefone" value={tutor.phone} />
                  <InfoRow label="Cadastrado em" value={formatDate(tutor.createdAt)} />
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Faturamento</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {tutor.billing ? (
                    <>
                      <InfoRow label="CPF/CNPJ" value={tutor.billing.document} />
                      {tutor.billing.billingEmail && (
                        <InfoRow label="E-mail de cobrança" value={tutor.billing.billingEmail} />
                      )}
                      {(tutor.billing.addressLine1 || tutor.billing.city) && (
                        <p className="border-t pt-2 text-muted-foreground">
                          {[
                            tutor.billing.addressLine1,
                            tutor.billing.addressLine2,
                            [tutor.billing.city, tutor.billing.state].filter(Boolean).join("/"),
                            tutor.billing.postalCode,
                          ]
                            .filter(Boolean)
                            .join(", ")}
                        </p>
                      )}
                    </>
                  ) : (
                    <p className="text-muted-foreground">Sem dados de faturamento.</p>
                  )}
                </CardContent>
              </Card>
            </div>
          }
        >
          {(tutor.emergencyContacts.length > 0 || tutor.authorizedPickups.length > 0) && (
            <div className="mb-6 grid gap-4 sm:grid-cols-2">
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Contatos de emergência</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {tutor.emergencyContacts.length === 0 ? (
                    <p className="text-muted-foreground">Nenhum.</p>
                  ) : (
                    tutor.emergencyContacts.map((c, i) => (
                      <div key={i} className="flex justify-between gap-4">
                        <span className="font-medium">
                          {c.name}
                          {c.relationship && (
                            <span className="ml-1 font-normal text-muted-foreground">({c.relationship})</span>
                          )}
                        </span>
                        <span className="text-right text-muted-foreground">{c.phone}</span>
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Autorizados a retirar</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {tutor.authorizedPickups.length === 0 ? (
                    <p className="text-muted-foreground">Nenhum.</p>
                  ) : (
                    tutor.authorizedPickups.map((p, i) => (
                      <div key={i} className="flex justify-between gap-4">
                        <span className="font-medium">{p.name}</span>
                        <span className="text-right text-muted-foreground">{p.document ?? "—"}</span>
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>
            </div>
          )}

          <div className="mb-6">
            <TutorConsentsCard tutorId={tutor.id} consents={tutor.consents} />
          </div>

          <div className="mb-6">
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Relatórios</CardTitle>
              </CardHeader>
              <CardContent>
                <TutorReportsCard tutorId={tutor.id} />
              </CardContent>
            </Card>
          </div>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">Pets</h2>
              <Button size="sm" onClick={() => navigate(`/registry/pets/new?tutorId=${tutor.id}`)}>
                <Plus /> Novo pet
              </Button>
            </div>
            <AsyncBoundary
              query={petsQuery}
              isEmpty={(data) => data.pages.every((p) => p.items.length === 0)}
              empty={
                <p className="rounded-lg border border-dashed py-8 text-center text-sm text-muted-foreground">
                  Nenhum pet cadastrado para este tutor.
                </p>
              }
            >
              {(data) => {
                const pets = data.pages.flatMap((p) => p.items);
                return (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Nome</TableHead>
                        <TableHead>Espécie</TableHead>
                        <TableHead>Raça</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {pets.map((pet) => (
                        <TableRow
                          key={pet.id}
                          data-clickable="true"
                          onClick={() => navigate(`/registry/pets/${pet.id}`)}
                        >
                          <TableCell className="font-medium">{pet.name}</TableCell>
                          <TableCell>{SPECIES_LABELS[pet.species as keyof typeof SPECIES_LABELS] ?? pet.species}</TableCell>
                          <TableCell className="text-muted-foreground">{pet.breed ?? "—"}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                );
              }}
            </AsyncBoundary>
          </div>
        </DetailPage>
        <ConfirmDialog
          open={confirmOpen}
          loading={deleteTutor.isPending}
          title="Excluir tutor"
          description={
            <>
              <p>
                O tutor <strong>{tutor.fullName}</strong> será excluído permanentemente. Esta ação é
                irreversível.
              </p>
              {deleteTutor.error instanceof ApiError && (
                <p className="text-destructive">{deleteTutor.error.message}</p>
              )}
            </>
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
