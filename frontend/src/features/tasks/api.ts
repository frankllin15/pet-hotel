import { apiClient, unwrap } from "@/shared/api/client";
import type { components } from "@/shared/api/schema";

export type OperationalTaskDto = components["schemas"]["OperationalTaskDto"];
export type TaskCategory = components["schemas"]["TaskCategory"];

export interface TaskInput {
  title: string;
  category: TaskCategory;
  assignedTo?: string | null;
}

export async function listTasks(date: string): Promise<OperationalTaskDto[]> {
  return unwrap(await apiClient.GET("/v1/tasks", { params: { query: { date } } }));
}

export async function createTask(date: string, input: TaskInput): Promise<{ id: string }> {
  return unwrap(
    await apiClient.POST("/v1/tasks", {
      body: { date, title: input.title, category: input.category, assignedTo: input.assignedTo ?? null },
    }),
  );
}

export async function updateTask(id: string, input: TaskInput): Promise<void> {
  unwrap(
    await apiClient.PUT("/v1/tasks/{id}", {
      params: { path: { id } },
      body: { id, title: input.title, category: input.category, assignedTo: input.assignedTo ?? null },
    }),
  );
}

export async function setTaskDone(id: string, done: boolean): Promise<void> {
  unwrap(await apiClient.POST("/v1/tasks/{id}/done", { params: { path: { id } }, body: { done } }));
}

export async function deleteTask(id: string): Promise<void> {
  unwrap(await apiClient.DELETE("/v1/tasks/{id}", { params: { path: { id } } }));
}
