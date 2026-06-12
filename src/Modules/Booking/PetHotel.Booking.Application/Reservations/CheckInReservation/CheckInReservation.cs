using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Application.Reservations.CheckInReservation;

/// <summary>Registra a entrada do pet (check-in) de uma reserva confirmada, com o estado de chegada opcional.</summary>
public sealed record CheckInReservation(Guid ReservationId, ArrivalStateInput? ArrivalState = null);

/// <summary>Estado do pet na chegada, informado no check-in.</summary>
public sealed record ArrivalStateInput(
    decimal? WeightKg,
    ArrivalCondition Condition,
    string? Observations);