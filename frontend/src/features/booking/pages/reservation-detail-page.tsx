import { useNavigate, useParams } from "react-router-dom";
import { formatDate, formatDateTime, formatMoney } from "@/shared/lib/format";
import { AsyncBoundary } from "@/shared/ui/async-boundary";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card";
import { DetailPage } from "@/shared/ui/archetypes/detail-page";
import { CareLogPanel } from "@/features/operations/components/care-log-panel";
import { ArrivalPhotos } from "../components/arrival-photos";
import { PetName } from "../components/pet-name";
import {
  ReservationActionButtons,
  ReservationActionError,
  useReservationActions,
} from "../components/reservation-actions";
import { ReservationStatusBadge } from "../components/reservation-status-badge";
import { useAccommodations, useReservation } from "../queries";
import { ARRIVAL_CONDITION_LABELS } from "../schemas";

export function ReservationDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const query = useReservation(id);
  const accommodations = useAccommodations();
  const actions = useReservationActions();

  const accommodationName = (accId: string) =>
    accommodations.data?.find((a) => a.id === accId)?.name ?? "—";

  return (
    <AsyncBoundary query={query} isEmpty={() => false}>
      {(r) => (
        <>
          <DetailPage
            title={
              <span className="flex items-center gap-3">
                <PetName petId={r.petId} />
                <ReservationStatusBadge status={r.status} />
              </span>
            }
            description={`Hospedagem de ${formatDate(r.checkIn)} a ${formatDate(r.checkOut)}`}
            actions={
              <div className="flex flex-wrap gap-2">
                <ReservationActionButtons
                  reservation={r}
                  onAction={actions.trigger}
                  busy={actions.busy}
                  size="default"
                />
                <Button variant="outline" onClick={() => navigate(`/registry/pets/${r.petId}`)}>
                  Ver pet
                </Button>
                <Button variant="outline" onClick={() => navigate("/booking/reservations")}>
                  Voltar
                </Button>
              </div>
            }
            sidePanel={
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm">Reserva</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  <InfoRow label="Acomodação" value={accommodationName(r.accommodationId)} />
                  <div className="flex justify-between gap-4">
                    <span className="text-muted-foreground">Status</span>
                    <ReservationStatusBadge status={r.status} />
                  </div>
                  <InfoRow label="Check-in previsto" value={formatDate(r.checkIn)} />
                  <InfoRow label="Check-out previsto" value={formatDate(r.checkOut)} />
                  <InfoRow label="Entrada real" value={formatDateTime(r.checkedInAt)} />
                  <InfoRow label="Saída real" value={formatDateTime(r.checkedOutAt)} />
                  <div className="border-t pt-2">
                    <InfoRow label={`Diária × ${r.nights} ${Number(r.nights) === 1 ? "noite" : "noites"}`} value={formatMoney(r.dailyRate)} />
                    <div className="mt-1 flex justify-between gap-4">
                      <span className="font-medium">Total</span>
                      <span className="text-right font-semibold">{formatMoney(r.totalAmount)}</span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            }
          >
            {actions.actionError && (
              <ReservationActionError message={actions.actionError} onClose={actions.clearError} />
            )}
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Estado de chegada</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                {r.arrivalState ? (
                  <>
                    <InfoRow
                      label="Condição"
                      value={ARRIVAL_CONDITION_LABELS[r.arrivalState.condition] ?? r.arrivalState.condition}
                    />
                    <InfoRow
                      label="Peso na chegada"
                      value={r.arrivalState.weightKg != null ? `${r.arrivalState.weightKg} kg` : "—"}
                    />
                    {r.arrivalState.observations && (
                      <div className="border-t pt-2">
                        <p className="mb-1 text-xs font-medium uppercase tracking-wider text-muted-foreground">
                          Observações
                        </p>
                        <p className="text-muted-foreground">{r.arrivalState.observations}</p>
                      </div>
                    )}
                  </>
                ) : (
                  <p className="text-muted-foreground">
                    {r.status === "Requested" || r.status === "Confirmed"
                      ? "O estado de chegada é registrado no momento do check-in."
                      : "Nenhum estado de chegada foi registrado nesta reserva."}
                  </p>
                )}
              </CardContent>
            </Card>
            <Card className="mt-6">
              <CardHeader>
                <CardTitle className="text-sm">Fotos de chegada</CardTitle>
              </CardHeader>
              <CardContent>
                <ArrivalPhotos
                  reservationId={r.id}
                  photoUrls={r.arrivalPhotoUrls}
                  canManage={r.status === "CheckedIn" || r.status === "CheckedOut"}
                />
              </CardContent>
            </Card>
            <Card className="mt-6">
              <CardHeader>
                <CardTitle className="text-sm">Diário de bordo</CardTitle>
              </CardHeader>
              <CardContent>
                <CareLogPanel
                  reservationId={r.id}
                  canManage={r.status === "CheckedIn" || r.status === "CheckedOut"}
                />
              </CardContent>
            </Card>
          </DetailPage>
          {actions.dialog}
        </>
      )}
    </AsyncBoundary>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4">
      <span className="text-muted-foreground">{label}</span>
      <span className="text-right font-medium">{value}</span>
    </div>
  );
}
