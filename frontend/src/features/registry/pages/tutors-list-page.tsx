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
import { useTutors } from "../queries";

export function TutorsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search.trim());
  const query = useTutors(debouncedSearch || undefined);

  return (
    <ListPage
      title="Tutores"
      description="Donos dos pets hospedados."
      primaryAction={
        <Button onClick={() => navigate("/registry/tutors/new")}>
          <Plus /> Novo tutor
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
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhum tutor encontrado.</p>}
      >
        {(data) => {
          const rows = data.pages.flatMap((p) => p.items);
          return (
            <div className="space-y-3">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nome</TableHead>
                    <TableHead>E-mail</TableHead>
                    <TableHead>Telefone</TableHead>
                    <TableHead>Cadastrado em</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {rows.map((tutor) => (
                    <TableRow
                      key={tutor.id}
                      data-clickable="true"
                      onClick={() => navigate(`/registry/tutors/${tutor.id}`)}
                    >
                      <TableCell className="font-medium">{tutor.fullName}</TableCell>
                      <TableCell>{tutor.email}</TableCell>
                      <TableCell>{tutor.phone}</TableCell>
                      <TableCell className="text-muted-foreground">{formatDate(tutor.createdAt)}</TableCell>
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
