namespace PetHotel.Operations.Application.Incidents;

/// <summary>Projeção de leitura de um incidente (docs/04).</summary>
public sealed record IncidentDto(
    Guid Id,
    string Severity,
    string Description,
    DateTimeOffset OccurredAt,
    string? ReportedBy,
    DateTimeOffset CreatedAt);
