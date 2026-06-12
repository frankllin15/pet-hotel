import { type RouteObject } from "react-router-dom";
import { AppLayout } from "@/app/app-layout";
import { RequireAuth } from "@/shared/auth/guards";
import { PlaceholderPage } from "@/shared/ui/placeholder-page";
import { LoginPage } from "@/features/auth/pages/login-page";
import { DashboardHomePage } from "@/features/dashboard/pages/dashboard-page";
import { TutorsListPage } from "@/features/registry/pages/tutors-list-page";
import { TutorFormPage } from "@/features/registry/pages/tutor-form-page";
import { TutorDetailPage } from "@/features/registry/pages/tutor-detail-page";
import { PetsListPage } from "@/features/registry/pages/pets-list-page";
import { PetFormPage } from "@/features/registry/pages/pet-form-page";
import { PetDetailPage } from "@/features/registry/pages/pet-detail-page";
import { ReservationsPage } from "@/features/booking/pages/reservations-page";
import { ReservationFormPage } from "@/features/booking/pages/reservation-form-page";
import { ReservationDetailPage } from "@/features/booking/pages/reservation-detail-page";
import { AccommodationsPage } from "@/features/booking/pages/accommodations-page";
import { OccupancyPage } from "@/features/booking/pages/occupancy-page";

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
          { path: "registry/tutors", element: <TutorsListPage /> },
          { path: "registry/tutors/new", element: <TutorFormPage /> },
          { path: "registry/tutors/:id", element: <TutorDetailPage /> },
          { path: "registry/tutors/:id/edit", element: <TutorFormPage /> },
          { path: "registry/pets", element: <PetsListPage /> },
          { path: "registry/pets/new", element: <PetFormPage /> },
          { path: "registry/pets/:id", element: <PetDetailPage /> },
          { path: "registry/pets/:id/edit", element: <PetFormPage /> },
          { path: "booking/reservations", element: <ReservationsPage /> },
          { path: "booking/reservations/new", element: <ReservationFormPage /> },
          { path: "booking/reservations/:id", element: <ReservationDetailPage /> },
          { path: "booking/accommodations", element: <AccommodationsPage /> },
          { path: "booking/occupancy", element: <OccupancyPage /> },
          { path: "*", element: <PlaceholderPage title="Página não encontrada" /> },
        ],
      },
    ],
  },
];
