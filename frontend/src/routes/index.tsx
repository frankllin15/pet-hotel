import { type RouteObject } from "react-router-dom";
import { AppLayout } from "@/app/app-layout";
import { RequireAuth } from "@/shared/auth/guards";
import { PlaceholderPage } from "@/shared/ui/placeholder-page";
import { LoginPage } from "@/features/auth/pages/login-page";
import { DashboardHomePage } from "@/features/dashboard/pages/dashboard-page";

/**
 * Árvore de rotas + guards (docs/08 §Estrutura). Rotas por feature, espelhando
 * os bounded contexts. Telas ainda não construídas usam <PlaceholderPage>.
 */
export const routes: RouteObject[] = [
  { path: "/login", element: <LoginPage /> },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <DashboardHomePage /> },
          { path: "registry/tutors", element: <PlaceholderPage title="Tutores" phase="Fase 1 — Registry" /> },
          { path: "registry/pets", element: <PlaceholderPage title="Pets" phase="Fase 1 — Registry" /> },
          { path: "health", element: <PlaceholderPage title="Saúde" phase="Fase 1 — Health" /> },
          { path: "booking/occupancy", element: <PlaceholderPage title="Ocupação" phase="Fase 1 — Booking" /> },
          { path: "*", element: <PlaceholderPage title="Página não encontrada" /> },
        ],
      },
    ],
  },
];
