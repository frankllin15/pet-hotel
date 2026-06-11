import { type ReactNode } from "react";
import { cn } from "@/shared/lib/utils";

export interface TabItem<T extends string> {
  value: T;
  label: ReactNode;
}

/** Barra de abas controlada (sem Radix). Usada no slot `tabs` do arquétipo Detalhe. */
export function TabBar<T extends string>({
  items,
  value,
  onChange,
}: {
  items: TabItem<T>[];
  value: T;
  onChange: (value: T) => void;
}) {
  return (
    <div className="-mb-px flex gap-1">
      {items.map((item) => (
        <button
          key={item.value}
          type="button"
          onClick={() => onChange(item.value)}
          className={cn(
            "border-b-2 px-4 py-2 text-sm font-medium transition-colors",
            value === item.value
              ? "border-primary font-semibold text-foreground"
              : "border-transparent text-muted-foreground hover:border-border hover:text-foreground",
          )}
        >
          {item.label}
        </button>
      ))}
    </div>
  );
}
