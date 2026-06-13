using PetHotel.Operations.Domain.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.Incidents.Events;

/// <summary>
/// Um incidente foi registrado. Futuro consumo: o Notifications aciona o tutor via Outbox
/// (especialmente para gravidade alta).
/// </summary>
public sealed record IncidentReported(
    IncidentId IncidentId, TenantId TenantId, PetReference Pet, IncidentSeverity Severity) : IDomainEvent;
