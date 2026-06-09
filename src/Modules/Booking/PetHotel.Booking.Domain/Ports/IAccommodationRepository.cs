using PetHotel.Booking.Domain.Accommodations;

namespace PetHotel.Booking.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="Accommodation"/>.</summary>
public interface IAccommodationRepository
{
    Task<Accommodation?> FindAsync(AccommodationId id, CancellationToken cancellationToken = default);

    void Add(Accommodation accommodation);
}
