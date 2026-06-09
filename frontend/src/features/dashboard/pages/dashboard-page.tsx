import { CalendarCheck, LogIn, LogOut, Pill } from "lucide-react";
import { useAuth } from "@/shared/auth/auth-context";
import { DashboardGrid, DashboardPage } from "@/shared/ui/archetypes/dashboard-page";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";

// Cards do painel — placeholders até as features de Fase 2 (docs/09).
const CARDS = [
  { label: "Chegadas hoje", value: "—", icon: LogIn },
  { label: "Saídas hoje", value: "—", icon: LogOut },
  { label: "Medicações pendentes", value: "—", icon: Pill },
  { label: "Reservas a confirmar", value: "—", icon: CalendarCheck },
];

export function DashboardHomePage() {
  const { claims } = useAuth();
  return (
    <DashboardPage
      title="Painel do dia"
      description={claims?.email ? `Bem-vindo, ${claims.email}.` : undefined}
    >
      <DashboardGrid>
        {CARDS.map((card) => (
          <Card key={card.label}>
            <CardHeader className="flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {card.label}
              </CardTitle>
              <card.icon className="size-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-semibold">{card.value}</p>
            </CardContent>
          </Card>
        ))}
      </DashboardGrid>
    </DashboardPage>
  );
}
