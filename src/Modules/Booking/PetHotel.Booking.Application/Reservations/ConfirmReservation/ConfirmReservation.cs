namespace PetHotel.Booking.Application.Reservations.ConfirmReservation;

/// <summary>Confirma uma reserva solicitada (consome o clearance sanitário do Health).</summary>
public sealed record ConfirmReservation(Guid ReservationId);
