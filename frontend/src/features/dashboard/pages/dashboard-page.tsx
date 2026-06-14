import { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { BedDouble, LogIn, LogOut, Pill, Syringe, type LucideIcon } from "lucide-react";
import { useAuth } from "@/shared/auth/auth-context";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { DashboardGrid, DashboardPage } from "@/shared/ui/archetypes/dashboard-page";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { formatDate, formatTime } from "@/shared/lib/format";
import { PetName } from "@/features/booking/components/pet-name";
import { useAccommodations } from "@/features/booking/queries";
import type { ReservationDto } from "@/features/booking/api";
import type { DashboardResponse, DayMedicationDto, ExpiringVaccinationDto } from "../api";
import { useDashboard } from "../queries";

export function DashboardHomePage() {
  const { claims } = useAuth();
  const dashboardQuery = useDashboard();
  const accommodationsQuery = useAccommodations();

  const accommodationName = useMemo(() => {
    const map = new Map((accommodationsQuery.data ?? []).map((a) => [a.id, a.name]));
    return (id: string) => map.get(id) ?? "Acomodação";
  }, [accommodationsQuery.data]);

  return (
    <DashboardPage
      title="Painel do dia"
      description={claims?.email ? `Bem-vindo, ${claims.email}.` : undefined}
    >
      <AsyncBoundary query={dashboardQuery} isEmpty={() => false}>
        {(data) => <DashboardContent data={data} accommodationName={accommodationName} />}
      </AsyncBoundary>
    </DashboardPage>
  );
}

function DashboardContent({
  data,
  accommodationName,
}: {
  data: DashboardResponse;
  accommodationName: (id: string) => string;
}) {
  const { board, medications, expiringVaccinations } = data;
  const occupied = Number(board.occupiedSlots);
  const total = Number(board.totalSlots);
  const occupancyPct = total > 0 ? Math.round((occupied / total) * 100) : 0;

  // Mapa estadia→pet para resolver o pet das medicações do dia.
  const petByReservation = useMemo(() => {
    const all = [...board.arrivals, ...board.departures, ...board.inHouse];
    return new Map(all.map((r) => [r.id, r.petId]));
  }, [board]);

  return (
    <div className="space-y-6">
      <DashboardGrid>
        <StatCard label="Chegadas hoje" value={board.arrivals.length} icon={LogIn} />
        <StatCard label="Saídas hoje" value={board.departures.length} icon={LogOut} />
        <StatCard label="Em estadia" value={board.inHouse.length} icon={BedDouble} />
        <StatCard
          label="Ocupação"
          value={`${occupancyPct}%`}
          hint={`${occupied} de ${total} vagas`}
          icon={BedDouble}
        />
        <StatCard label="Medicações hoje" value={medications.length} icon={Pill} />
        <StatCard label="Vacinas vencendo" value={expiringVaccinations.length} icon={Syringe} />
      </DashboardGrid>

      <div className="grid gap-4 lg:grid-cols-2">
        <ReservationListCard
          title="Chegadas previstas"
          empty="Nenhuma chegada para hoje."
          reservations={board.arrivals}
          accommodationName={accommodationName}
        />
        <ReservationListCard
          title="Saídas previstas"
          empty="Nenhuma saída para hoje."
          reservations={board.departures}
          accommodationName={accommodationName}
        />
        <MedicationsCard medications={medications} petByReservation={petByReservation} />
        <VaccineAlertsCard alerts={expiringVaccinations} date={data.date} />
      </div>
    </div>
  );
}

function StatCard({
  label,
  value,
  hint,
  icon: Icon,
}: {
  label: string;
  value: number | string;
  hint?: string;
  icon: LucideIcon;
}) {
  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
        <Icon className="size-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-semibold">{value}</p>
        {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
      </CardContent>
    </Card>
  );
}

function ReservationListCard({
  title,
  empty,
  reservations,
  accommodationName,
}: {
  title: string;
  empty: string;
  reservations: ReservationDto[];
  accommodationName: (id: string) => string;
}) {
  const navigate = useNavigate();
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm">{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-1">
        {reservations.length === 0 ? (
          <p className="py-4 text-center text-sm text-muted-foreground">{empty}</p>
        ) : (
          reservations.map((r) => (
            <button
              key={r.id}
              type="button"
              onClick={() => navigate(`/booking/reservations/${r.id}`)}
              className="flex w-full items-center justify-between rounded-md px-2 py-2 text-left text-sm hover:bg-muted"
            >
              <span className="font-medium">
                <PetName petId={r.petId} />
              </span>
              <span className="text-muted-foreground">{accommodationName(r.accommodationId)}</span>
            </button>
          ))
        )}
      </CardContent>
    </Card>
  );
}

function MedicationsCard({
  medications,
  petByReservation,
}: {
  medications: DayMedicationDto[];
  petByReservation: Map<string, string>;
}) {
  const navigate = useNavigate();
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm">Medicações de hoje</CardTitle>
      </CardHeader>
      <CardContent className="space-y-1">
        {medications.length === 0 ? (
          <p className="py-4 text-center text-sm text-muted-foreground">Nenhuma medicação hoje.</p>
        ) : (
          medications.map((m) => {
            const petId = petByReservation.get(m.reservationId);
            return (
              <button
                key={m.id}
                type="button"
                onClick={() => navigate(`/booking/reservations/${m.reservationId}`)}
                className="flex w-full items-center justify-between gap-3 rounded-md px-2 py-2 text-left text-sm hover:bg-muted"
              >
                <span className="flex items-center gap-2">
                  <span className="tabular-nums text-muted-foreground">{formatTime(m.administeredAt)}</span>
                  <span className="font-medium">{petId ? <PetName petId={petId} /> : "—"}</span>
                </span>
                <span className="text-right text-muted-foreground">
                  {m.drug} — {m.dose}
                </span>
              </button>
            );
          })
        )}
      </CardContent>
    </Card>
  );
}

function VaccineAlertsCard({ alerts, date }: { alerts: ExpiringVaccinationDto[]; date: string }) {
  const navigate = useNavigate();
  const today = date;
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm">Alertas de vacina</CardTitle>
      </CardHeader>
      <CardContent className="space-y-1">
        {alerts.length === 0 ? (
          <p className="py-4 text-center text-sm text-muted-foreground">Nenhuma vacina vencendo.</p>
        ) : (
          alerts.map((v) => {
            const expired = v.expiresOn < today;
            return (
              <button
                key={`${v.petId}-${v.vaccineType}`}
                type="button"
                onClick={() => navigate(`/registry/pets/${v.petId}`)}
                className="flex w-full items-center justify-between gap-3 rounded-md px-2 py-2 text-left text-sm hover:bg-muted"
              >
                <span className="font-medium">
                  <PetName petId={v.petId} />
                </span>
                <span className="flex items-center gap-2 text-muted-foreground">
                  <span>{v.vaccineType}</span>
                  <span className={expired ? "text-destructive" : "text-amber-600"}>
                    {expired ? "vencida" : "vence"} {formatDate(v.expiresOn)}
                  </span>
                </span>
              </button>
            );
          })
        )}
      </CardContent>
    </Card>
  );
}
