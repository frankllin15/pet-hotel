import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { usePack, useDeletePack } from "../queries";
import { COMPAT_FLAG_LABELS } from "../schemas";

export function PackDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const query = usePack(id);
  const deletePack = useDeletePack();
  const [confirmOpen, setConfirmOpen] = useState(false);

  async function runDelete() {
    try {
      await deletePack.mutateAsync(id);
      navigate("/registry/packs");
    } catch {
      // Erro exibido no diálogo via deletePack.error.
    }
  }

  return (
    <AsyncBoundary query={query} isEmpty={() => false}>
      {(pack) => (
        <>
          <DetailPage
            title={
              <span className="flex items-center gap-3">
                {pack.name}
                {pack.needsAttention ? (
                  <Badge variant="warning">Requer atenção</Badge>
                ) : (
                  <Badge variant="success">Compatível</Badge>
                )}
              </span>
            }
            description="Matilha"
            actions={
              <div className="flex gap-2">
                <Button onClick={() => navigate(`/registry/packs/${pack.id}/edit`)}>Editar</Button>
                <Button
                  variant="destructive"
                  onClick={() => {
                    deletePack.reset();
                    setConfirmOpen(true);
                  }}
                >
                  Excluir
                </Button>
                <Button variant="outline" onClick={() => navigate("/registry/packs")}>
                  Voltar
                </Button>
              </div>
            }
            sidePanel={
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Observações</CardTitle>
                </CardHeader>
                <CardContent className="text-sm text-muted-foreground">
                  {pack.notes ?? "Sem observações."}
                </CardContent>
              </Card>
            }
          >
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Membros ({pack.members.length})</CardTitle>
              </CardHeader>
              <CardContent>
                {pack.members.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Nenhum pet nesta matilha.</p>
                ) : (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Pet</TableHead>
                        <TableHead>Espécie</TableHead>
                        <TableHead>Compatibilidade</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {pack.members.map((m) => (
                        <TableRow
                          key={m.petId}
                          data-clickable={m.found ? "true" : undefined}
                          onClick={m.found ? () => navigate(`/registry/pets/${m.petId}`) : undefined}
                        >
                          <TableCell className="font-medium">{m.name}</TableCell>
                          <TableCell className="text-muted-foreground">{m.species ?? "—"}</TableCell>
                          <TableCell>
                            {m.flags.length === 0 ? (
                              <span className="text-sm text-muted-foreground">Sem alertas</span>
                            ) : (
                              <span className="flex flex-wrap gap-1">
                                {m.flags.map((f) => (
                                  <Badge key={f} variant="warning">
                                    {COMPAT_FLAG_LABELS[f] ?? f}
                                  </Badge>
                                ))}
                              </span>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </CardContent>
            </Card>
          </DetailPage>
          <ConfirmDialog
            open={confirmOpen}
            loading={deletePack.isPending}
            title="Excluir matilha"
            description={
              <>
                <p>
                  A matilha <strong>{pack.name}</strong> será excluída. Os pets não são afetados.
                </p>
                {deletePack.error instanceof ApiError && (
                  <p className="text-destructive">{deletePack.error.message}</p>
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
