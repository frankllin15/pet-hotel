using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Incidents.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.Incidents;

/// <summary>
/// Incidente ocorrido durante a estadia (auditável). Levanta <see cref="IncidentReported"/> para
/// futura notificação ao tutor (Outbox/Notifications). Vinculado ao contexto de presença.
/// </summary>
public sealed class Incident : AggregateRoot<IncidentId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public PetReference Pet { get; private set; }
    public CareContextType ContextType { get; private set; }
    public Guid ContextId { get; private set; }
    public IncidentSeverity Severity { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Incident() { } // EF

    private Incident(
        IncidentId id, TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        IncidentSeverity severity, string description, DateTimeOffset occurredAt) : base(id)
    {
        TenantId = tenantId;
        Pet = pet;
        ContextType = contextType;
        ContextId = contextId;
        Severity = severity;
        Description = description;
        OccurredAt = occurredAt;
    }

    public static Result<Incident> Report(
        TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        IncidentSeverity severity, string? description, DateTimeOffset occurredAt, DateTimeOffset now)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("incident.tenant_required", "Tenant é obrigatório.");
        }

        if (pet.Value == Guid.Empty || contextId == Guid.Empty)
        {
            return Error.Validation("incident.context_required", "Estadia é obrigatória.");
        }

        if (!Enum.IsDefined(severity))
        {
            return Error.Validation("incident.severity_invalid", "Gravidade inválida.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("incident.description_required", "Descrição é obrigatória.");
        }

        if (occurredAt > now)
        {
            return Error.Validation("incident.occurred_in_future", "O incidente não pode estar no futuro.");
        }

        var incident = new Incident(
            IncidentId.New(), tenantId, pet, contextType, contextId, severity, description.Trim(), occurredAt);
        incident.Raise(new IncidentReported(incident.Id, tenantId, pet, severity));
        return incident;
    }
}
