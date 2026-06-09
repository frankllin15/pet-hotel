namespace PetHotel.Booking.Domain.Accommodations;

/// <summary>Identificador tipado de acomodação.</summary>
public readonly record struct AccommodationId(Guid Value)
{
    public static AccommodationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
