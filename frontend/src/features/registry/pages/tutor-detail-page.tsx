import { useNavigate, useParams } from "react-router-dom";
import { Plus } from "lucide-react";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { SPECIES_LABELS } from "../schemas";
import { usePets, useTutor } from "../queries";

export function TutorDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const tutorQuery = useTutor(id);
  const petsQuery = usePets({ tutorId: id });

  return (
    <AsyncBoundary query={tutorQuery} isEmpty={() => false}>
      {(tutor) => (
        <DetailPage
          title={tutor.fullName}
          description="Ficha do tutor"
          actions={
            <Button variant="outline" onClick={() => navigate("/registry/tutors")}>
              Voltar
            </Button>
          }
          sidePanel={
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
          }
        >
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
