import { z } from "zod";

/** Categorias de tarefa (valores batem com TaskCategory do backend). */
export const TASK_CATEGORIES = ["Cleaning", "Feeding", "Recreation", "Other"] as const;
export const TASK_CATEGORY_LABELS: Record<string, string> = {
  Cleaning: "Limpeza",
  Feeding: "Alimentação",
  Recreation: "Recreação",
  Other: "Outros",
};

export const taskFormSchema = z.object({
  title: z.string().min(1, "Informe o título").max(200),
  category: z.enum(TASK_CATEGORIES),
  assignedTo: z.string(), // "" = sem responsável
});
export type TaskFormInput = z.infer<typeof taskFormSchema>;
