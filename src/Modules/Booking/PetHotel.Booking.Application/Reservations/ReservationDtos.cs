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

/// <summary>
/// Painel do dia do Booking: chegadas e saídas previstas, pets em estadia e ocupação por vaga.
/// "Chegadas" = confirmadas com check-in no dia (ainda não chegaram). "Saídas" = em estadia
/// com check-out no dia. "EmEstadia" = todas em CheckedIn. Ocupação = vagas ocupadas (reservas
/// ativas cobrindo o dia) sobre a capacidade total das acomodações disponíveis.
/// </summary>
public sealed record DayBoardDto(
    IReadOnlyList<ReservationDto> Arrivals,
    IReadOnlyList<ReservationDto> Departures,
    IReadOnlyList<ReservationDto> InHouse,
    int OccupiedSlots,
    int TotalSlots);
