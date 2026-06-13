using PetHotel.Operations.Domain.Incidents;

namespace PetHotel.Operations.Application.Incidents.ReportIncident;

/// <summary>Registra um incidente numa estadia. OccurredAt nulo = agora.</summary>
public sealed record ReportIncident(Guid ReservationId, IncidentSeverity Severity, string Description, DateTimeOffset? OccurredAt);
