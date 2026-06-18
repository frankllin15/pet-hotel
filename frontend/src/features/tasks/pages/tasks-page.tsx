import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";
import { ListPage } from "@/shared/ui/archetypes/list-page";
import { Modal } from "@/shared/ui/modal";
import { Select } from "@/shared/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/shared/ui/table";
import { useUsers } from "@/features/users/queries";
import { UserName } from "@/features/users/components/user-name";
import type { OperationalTaskDto } from "../api";
import { useTaskMutations, useTasks } from "../queries";
import { TASK_CATEGORIES, TASK_CATEGORY_LABELS, taskFormSchema, type TaskFormInput } from "../schemas";

/** Data de hoje (local) no formato yyyy-MM-dd. */
const todayISO = () => new Date().toLocaleDateString("en-CA");

export function TasksPage() {
  const [date, setDate] = useState(todayISO);
  const [editing, setEditing] = useState<OperationalTaskDto | "new" | null>(null);

  const query = useTasks(date);
  const mutations = useTaskMutations(date);

  return (
    <ListPage
      title="Tarefas do dia"
      description="Checklist operacional do hotel (limpeza, alimentação, recreação)."
      primaryAction={
        <Button onClick={() => setEditing("new")}>
          <Plus /> Nova tarefa
        </Button>
      }
      filters={
        <label className="flex items-center gap-2 text-sm text-muted-foreground">
          Dia
          <Input type="date" className="w-44" value={date} onChange={(e) => setDate(e.target.value)} aria-label="Dia" />
        </label>
      }
    >
      <AsyncBoundary
        query={query}
        isEmpty={(data) => data.length === 0}
        empty={<p className="py-10 text-center text-sm text-muted-foreground">Nenhuma tarefa para este dia.</p>}
      >
        {(tasks) => (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-10"></TableHead>
                <TableHead>Tarefa</TableHead>
                <TableHead>Categoria</TableHead>
                <TableHead>Responsável</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {tasks.map((t) => (
                <TableRow key={t.id}>
                  <TableCell>
                    <input
                      type="checkbox"
                      className="size-4 accent-primary"
                      checked={t.done}
                      disabled={mutations.toggle.isPending}
                      onChange={(e) => mutations.toggle.mutate({ id: t.id, done: e.target.checked })}
                      aria-label={t.done ? "Marcar como pendente" : "Marcar como feita"}
                    />
                  </TableCell>
                  <TableCell className={t.done ? "text-muted-foreground line-through" : "font-medium"}>
                    {t.title}
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary">{TASK_CATEGORY_LABELS[t.category] ?? t.category}</Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    <UserName userId={t.assignedTo} />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button size="sm" variant="outline" onClick={() => setEditing(t)}>
                        <Pencil /> Editar
                      </Button>
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => mutations.remove.mutate(t.id)}
                        disabled={mutations.remove.isPending}
                        aria-label="Excluir tarefa"
                      >
                        <Trash2 />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </AsyncBoundary>

      {editing && (
        <TaskFormModal
          key={editing === "new" ? "new" : editing.id}
          task={editing === "new" ? undefined : editing}
          mutations={mutations}
          onClose={() => setEditing(null)}
        />
      )}
    </ListPage>
  );
}

function TaskFormModal({
  task,
  mutations,
  onClose,
}: {
  task?: OperationalTaskDto;
  mutations: ReturnType<typeof useTaskMutations>;
  onClose: () => void;
}) {
  const isEdit = !!task;
  const usersQuery = useUsers();
  const mutation = isEdit ? mutations.update : mutations.create;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TaskFormInput>({
    resolver: zodResolver(taskFormSchema),
    defaultValues: task
      ? { title: task.title, category: task.category as TaskFormInput["category"], assignedTo: task.assignedTo ?? "" }
      : { title: "", category: "Cleaning", assignedTo: "" },
  });

  const submit = handleSubmit((values) => {
    const input = { title: values.title, category: values.category, assignedTo: values.assignedTo || null };
    if (isEdit) {
      mutations.update.mutate({ id: task.id, input }, { onSuccess: onClose });
    } else {
      mutations.create.mutate(input, { onSuccess: onClose });
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <Modal
      open
      title={isEdit ? "Editar tarefa" : "Nova tarefa"}
      description="Tarefa operacional do dia."
      busy={mutation.isPending}
      onClose={onClose}
    >
      <form className="space-y-4" onSubmit={submit}>
        <Field label="Título" htmlFor="title" error={errors.title?.message}>
          <Input id="title" placeholder="Ex.: Limpar ala A" aria-invalid={!!errors.title} {...register("title")} />
        </Field>

        <div className="grid grid-cols-2 gap-3">
          <Field label="Categoria" htmlFor="category" error={errors.category?.message}>
            <Select id="category" {...register("category")}>
              {TASK_CATEGORIES.map((c) => (
                <option key={c} value={c}>
                  {TASK_CATEGORY_LABELS[c]}
                </option>
              ))}
            </Select>
          </Field>
          <Field label="Responsável" htmlFor="assignedTo">
            <Select id="assignedTo" {...register("assignedTo")}>
              <option value="">Sem responsável</option>
              {usersQuery.data?.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.displayName}
                </option>
              ))}
            </Select>
          </Field>
        </div>

        {formError && <p className="text-sm text-destructive">{formError}</p>}

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            Cancelar
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Salvar"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
