namespace PetHotel.Booking.Application.Reservations.CheckInReservation;

/// <summary>Registra a entrada do pet (check-in) de uma reserva confirmada.</summary>
public sealed record CheckInReservation(Guid ReservationId);