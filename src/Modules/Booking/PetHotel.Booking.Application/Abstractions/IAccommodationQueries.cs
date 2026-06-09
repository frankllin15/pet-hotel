using PetHotel.Booking.Application.Accommodations;

namespace PetHotel.Booking.Application.Abstractions;

/// <summary>Porta de leitura de acomodações (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IAccommodationQueries
{
    Task<IReadOnlyList<AccommodationDto>> ListAsync(CancellationToken cancellationToken = default);
}
