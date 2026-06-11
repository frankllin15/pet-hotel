import { NavLink, Outlet } from "react-router-dom";
import { CalendarCheck, CalendarDays, LayoutDashboard, LogOut, PawPrint, Users } from "lucide-react";
import { useAuth } from "@/shared/auth/auth-context";
import { type Role } from "@/shared/auth/roles";
import { FeatureErrorBoundary } from "@/shared/ui/error-boundary";
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
  { to: "/booking/reservations", label: "Reservas", icon: CalendarCheck },
  { to: "/booking/occupancy", label: "Ocupação", icon: CalendarDays },
];

/** Shell raiz: sidebar "couro" de navegação + área de conteúdo em papel. */
export function AppLayout() {
  const { claims, hasRole, signOut } = useAuth();
  const initial = (claims?.email ?? "?").charAt(0).toUpperCase();

  return (
    <div className="grid min-h-screen grid-cols-[17rem_1fr]">
      <aside className="sticky top-0 flex h-screen flex-col bg-sidebar text-sidebar-foreground">
        {/* Marca */}
        <div className="flex items-center gap-3 px-5 pb-5 pt-6">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-raised">
            <PawPrint className="size-5" />
          </div>
          <div className="min-w-0 leading-tight">
            <p className="font-display text-xl font-semibold tracking-tight">PetHotel</p>
            <p className="font-display text-xs italic text-sidebar-muted">hospedagem &amp; cuidado</p>
          </div>
        </div>

        <nav className="flex-1 space-y-1 px-3">
          <p className="px-3 pb-2 pt-1 text-[0.65rem] font-semibold uppercase tracking-[0.18em] text-sidebar-muted">
            Operação
          </p>
          {NAV.filter((item) => !item.roles || hasRole(...item.roles)).map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              className={({ isActive }) =>
                cn(
                  "group relative flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground shadow-raised"
                    : "text-sidebar-muted hover:bg-sidebar-accent hover:text-sidebar-foreground",
                )
              }
            >
              <item.icon className="size-4 shrink-0 transition-transform group-hover:-rotate-6" />
              {item.label}
            </NavLink>
          ))}
        </nav>

        {/* Sessão */}
        <div className="m-3 rounded-xl border border-sidebar-border bg-sidebar-accent/60 p-3">
          <div className="flex items-center gap-3">
            <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary/25 font-display text-sm font-semibold text-sidebar-foreground">
              {initial}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium">{claims?.email ?? "Sessão"}</p>
              <p className="truncate text-xs text-sidebar-muted">{claims?.roles.join(", ")}</p>
            </div>
            <button
              type="button"
              onClick={signOut}
              aria-label="Sair"
              title="Sair"
              className="flex size-8 shrink-0 items-center justify-center rounded-lg text-sidebar-muted transition-colors hover:bg-sidebar-accent hover:text-sidebar-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              <LogOut className="size-4" />
            </button>
          </div>
        </div>
      </aside>

      <main className="min-w-0 overflow-auto">
        <div className="mx-auto max-w-7xl p-8">
          <FeatureErrorBoundary feature="content">
            <Outlet />
          </FeatureErrorBoundary>
        </div>
      </main>
    </div>
  );
}
