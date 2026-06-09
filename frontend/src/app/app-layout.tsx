import { NavLink, Outlet } from "react-router-dom";
import { CalendarDays, LayoutDashboard, PawPrint, ShieldCheck, Users } from "lucide-react";
import { useAuth } from "@/shared/auth/auth-context";
import { type Role } from "@/shared/auth/roles";
import { FeatureErrorBoundary } from "@/shared/ui/error-boundary";
import { Button } from "@/shared/ui/button";
import { cn } from "@/shared/lib/utils";

interface NavItem {
  to: string;
  label: string;
  icon: typeof PawPrint;
  /** Papéis que enxergam o item; vazio = todos os autenticados. */
  roles?: Role[];
}

// Navegação espelha os bounded contexts (docs/08 §Estrutura).
const NAV: NavItem[] = [
  { to: "/", label: "Painel", icon: LayoutDashboard },
  { to: "/registry/tutors", label: "Tutores", icon: Users },
  { to: "/registry/pets", label: "Pets", icon: PawPrint },
  { to: "/health", label: "Saúde", icon: ShieldCheck },
  { to: "/booking/occupancy", label: "Ocupação", icon: CalendarDays },
];

/** Shell raiz: sidebar de navegação + área de conteúdo. */
export function AppLayout() {
  const { claims, hasRole, signOut } = useAuth();

  return (
    <div className="grid min-h-screen grid-cols-[16rem_1fr]">
      <aside className="flex flex-col border-r bg-card">
        <div className="flex h-14 items-center gap-2 border-b px-4 font-semibold">
          <PawPrint className="size-5 text-primary" />
          PetHotel
        </div>
        <nav className="flex-1 space-y-1 p-2">
          {NAV.filter((item) => !item.roles || hasRole(...item.roles)).map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-accent text-accent-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                )
              }
            >
              <item.icon className="size-4" />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t p-3 text-sm">
          <p className="truncate font-medium">{claims?.email ?? "Sessão"}</p>
          <p className="truncate text-xs text-muted-foreground">{claims?.roles.join(", ")}</p>
          <Button variant="ghost" size="sm" className="mt-2 w-full justify-start" onClick={signOut}>
            Sair
          </Button>
        </div>
      </aside>
      <main className="min-w-0 overflow-auto p-8">
        <FeatureErrorBoundary feature="content">
          <Outlet />
        </FeatureErrorBoundary>
      </main>
    </div>
  );
}
