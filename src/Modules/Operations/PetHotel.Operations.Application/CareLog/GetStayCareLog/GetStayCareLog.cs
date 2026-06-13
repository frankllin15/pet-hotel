namespace PetHotel.Operations.Application.CareLog.GetStayCareLog;

/// <summary>Timeline do diário de bordo de uma estadia/reserva (paginada por cursor).</summary>
public sealed record GetStayCareLog(Guid ReservationId, string? Cursor, int Limit);
