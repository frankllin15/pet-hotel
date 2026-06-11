import { type ReactNode } from "react";
import { PawPrint, type LucideIcon } from "lucide-react";

/** Estado vazio acolhedor: ícone em tile quente + título em display + ação opcional. */
export function EmptyState({
  icon: Icon = PawPrint,
  title,
  description,
  action,
}: {
  icon?: LucideIcon;
  title: ReactNode;
  description?: ReactNode;
  action?: ReactNode;
}) {
  return (
    <div className="rise-in flex flex-col items-center gap-3 rounded-xl border border-dashed bg-card/60 px-6 py-12 text-center">
      <div className="flex size-12 items-center justify-center rounded-xl bg-accent text-accent-foreground">
        <Icon className="size-6" />
      </div>
      <div className="space-y-1">
        <p className="font-display text-lg font-semibold">{title}</p>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      {action}
    </div>
  );
}
