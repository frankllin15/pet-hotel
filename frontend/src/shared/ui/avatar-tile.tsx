import { cn } from "@/shared/lib/utils";

/*
  Tile de identidade "ficha de hóspede": inicial em Fraunces sobre um tom
  quente derivado do nome (estável entre renderizações). Só tokens do tema.
*/
const TINTS = [
  "bg-primary/15 text-primary",
  "bg-success/15 text-success",
  "bg-warning/25 text-warning-foreground",
  "bg-accent text-accent-foreground",
] as const;

function tintOf(seed: string): string {
  let hash = 0;
  for (const char of seed) {
    hash = (hash * 31 + char.charCodeAt(0)) % 997;
  }
  return TINTS[hash % TINTS.length];
}

export function AvatarTile({
  name,
  size = "md",
  className,
}: {
  name: string;
  size?: "md" | "lg";
  className?: string;
}) {
  const initial = name.trim().charAt(0).toUpperCase() || "?";
  return (
    <span
      aria-hidden
      className={cn(
        "flex shrink-0 select-none items-center justify-center font-display font-semibold",
        size === "md" ? "size-9 rounded-lg text-sm" : "size-12 rounded-xl text-xl",
        tintOf(name),
        className,
      )}
    >
      {initial}
    </span>
  );
}
