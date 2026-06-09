namespace PetHotel.Booking.Domain.Reservations;

/// <summary>Ciclo de vida da reserva.</summary>
public enum ReservationStatus
{
    Requested,
    Confirmed,
    Cancelled
}
