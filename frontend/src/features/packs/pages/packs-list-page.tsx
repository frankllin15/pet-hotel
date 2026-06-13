import { useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { EmptyState } from "@/shared/ui/empty-state";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { usePacks } from "../queries";

export function PacksListPage() {
  const navigate = useNavigate();
  const query = usePacks();

  return (
    <ListPage
      title="Matilhas"
      description="Grupos de pets compatíveis para conviverem juntos."
      primaryAction={
        <Button onClick={() => navigate("/registry/packs/new")}>
          <Plus /> Nova matilha
        </Button>
      }
    >
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={
          <EmptyState
            title="Nenhuma matilha"
            description="Agrupe pets que se dão bem para facilitar a alocação."
            action={
              <Button size="sm" onClick={() => navigate("/registry/packs/new")}>
                <Plus /> Nova matilha
              </Button>
            }
          />
        }
      >
        {(packs) => (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Membros</TableHead>
                <TableHead>Compatibilidade</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {packs.map((p) => (
                <TableRow key={p.id} data-clickable="true" onClick={() => navigate(`/registry/packs/${p.id}`)}>
                  <TableCell className="font-medium">{p.name}</TableCell>
                  <TableCell>{Number(p.memberCount)}</TableCell>
                  <TableCell>
                    {p.needsAttention ? (
                      <Badge variant="warning">Requer atenção</Badge>
                    ) : (
                      <Badge variant="success">Compatível</Badge>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>
    </ListPage>
  );
}
