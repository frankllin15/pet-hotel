namespace PetHotel.Booking.Domain.Reservations;

/// <summary>Identificador tipado de reserva.</summary>
public readonly record struct ReservationId(Guid Value)
{
    public static ReservationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
