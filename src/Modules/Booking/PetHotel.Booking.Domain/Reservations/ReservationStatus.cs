namespace PetHotel.Booking.Domain.Reservations;

/// <summary>Ciclo de vida da reserva: Requested → Confirmed → CheckedIn → CheckedOut (Cancelled antes do check-in).</summary>
public enum ReservationStatus
{
    Requested,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}
