import { z } from "zod";

export const packFormSchema = z.object({
  name: z.string().min(1, "Informe o nome").max(120),
  notes: z.string().max(1000).optional().or(z.literal("")),
});
export type PackFormInput = z.infer<typeof packFormSchema>;

/** Rótulos pt-BR dos sinais de compatibilidade (valores = PackCompatibilityFlag do backend). */
export const COMPAT_FLAG_LABELS: Record<string, string> = {
  Reactive: "Reativo",
  LowSociability: "Pouco sociável",
};

/** Sinais de compatibilidade derivados da avaliação do pet (espelha o critério do backend). */
export function compatFlags(
  sociability: string | null | undefined,
  reactivity: string | null | undefined,
): string[] {
  const flags: string[] = [];
  if (reactivity === "High") flags.push("Reactive");
  if (sociability === "Low") flags.push("LowSociability");
  return flags;
}
