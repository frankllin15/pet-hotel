namespace PetHotel.Operations.Application.CareLog;

/// <summary>Projeção de leitura de uma entrada do diário de bordo (docs/04).</summary>
public sealed record CareLogEntryDto(
    Guid Id,
    Guid PetId,
    string Type,
    string? Note,
    DateTimeOffset OccurredAt,
    string? RegisteredBy,
    IReadOnlyList<string> PhotoUrls,
    DateTimeOffset CreatedAt);
