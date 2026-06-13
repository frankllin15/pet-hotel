using PetHotel.Operations.Domain.CareLog;

namespace PetHotel.Operations.Application.CareLog.LogCareEntry;

/// <summary>
/// Registra uma ocorrência no diário de bordo de uma estadia (reserva). O pet é derivado da
/// estadia. OccurredAt nulo = agora.
/// </summary>
public sealed record LogCareEntry(Guid ReservationId, CareLogEntryType Type, string? Note, DateTimeOffset? OccurredAt);
