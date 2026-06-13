namespace PetHotel.Booking.Application.Reservations;

/// <summary>Projeção de leitura de uma reserva.</summary>
public sealed record ReservationDto(
    Guid Id,
    Guid PetId,
    Guid AccommodationId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Status,
    DateTimeOffset? CheckedInAt,
    DateTimeOffset? CheckedOutAt,
    int Nights,
    decimal DailyRate,
    decimal TotalAmount,
    ArrivalStateDto? ArrivalState,
    IReadOnlyList<string> ArrivalPhotoUrls);

/// <summary>Estado do pet na chegada (leitura).</summary>
public sealed record ArrivalStateDto(
    decimal? WeightKg,
    string Condition,
    string? Observations);

/// <summary>Linha do calendário de ocupação (reservas confirmadas).</summary>
public sealed record OccupancyEntryDto(
    Guid AccommodationId,
    Guid ReservationId,
    Guid PetId,
    DateOnly CheckIn,
    DateOnly CheckOut);
