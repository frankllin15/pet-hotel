import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Search } from "lucide-react";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { formatDate } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { usePets } from "../queries";
import { SPECIES_LABELS } from "../schemas";

export function PetsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search.trim());
  const query = usePets({ search: debouncedSearch || undefined });

  return (
    <ListPage
      title="Pets"
      description="Animais cadastrados no hotel."
      primaryAction={
        <Button onClick={() => navigate("/registry/pets/new")}>
          <Plus /> Novo pet
        </Button>
      }
      filters={
        <div className="relative w-full max-w-xs">
          <Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
          <Input
            className="pl-8"
            placeholder="Buscar por nome…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      }
    >
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.pages.every((p) => p.items.length === 0)}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhum pet encontrado.</p>}
      >
        {(data) => {
          const rows = data.pages.flatMap((p) => p.items);
          return (
            <div className="space-y-3">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nome</TableHead>
                    <TableHead>Espécie</TableHead>
                    <TableHead>Raça</TableHead>
                    <TableHead>Cadastrado em</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {rows.map((pet) => (
                    <TableRow
                      key={pet.id}
                      data-clickable="true"
                      onClick={() => navigate(`/registry/pets/${pet.id}`)}
                    >
                      <TableCell className="font-medium">{pet.name}</TableCell>
                      <TableCell>{SPECIES_LABELS[pet.species as keyof typeof SPECIES_LABELS] ?? pet.species}</TableCell>
                      <TableCell>{pet.breed ?? "—"}</TableCell>
                      <TableCell className="text-muted-foreground">{formatDate(pet.createdAt)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              {query.hasNextPage && (
                <div className="flex justify-center">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => query.fetchNextPage()}
                    disabled={query.isFetchingNextPage}
                  >
                    {query.isFetchingNextPage ? "Carregando…" : "Carregar mais"}
                  </Button>
                </div>
              )}
            </div>
          );
        }}
      </AsyncBoundary>
    </ListPage>
  );
}
