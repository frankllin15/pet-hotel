import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createTask, deleteTask, listTasks, setTaskDone, updateTask, type TaskInput } from "./api";

export const taskKeys = {
  all: ["tasks"] as const,
  byDate: (date: string) => ["tasks", date] as const,
};

export function useTasks(date: string) {
  return useQuery({
    queryKey: taskKeys.byDate(date),
    queryFn: () => listTasks(date),
    enabled: !!date,
    placeholderData: (prev) => prev,
  });
}

/** Mutations da agenda de tarefas de um dia, invalidando a lista daquele dia. */
export function useTaskMutations(date: string) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: taskKeys.byDate(date) });

  const create = useMutation({ mutationFn: (input: TaskInput) => createTask(date, input), onSuccess: invalidate });
  const update = useMutation({
    mutationFn: ({ id, input }: { id: string; input: TaskInput }) => updateTask(id, input),
    onSuccess: invalidate,
  });
  const toggle = useMutation({
    mutationFn: ({ id, done }: { id: string; done: boolean }) => setTaskDone(id, done),
    onSuccess: invalidate,
  });
  const remove = useMutation({ mutationFn: (id: string) => deleteTask(id), onSuccess: invalidate });

  return { create, update, toggle, remove };
}
