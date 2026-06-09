using PetHotel.Booking.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Accommodations.ListAccommodations;

/// <summary>Projeta as acomodações do tenant via porta de leitura (docs/04).</summary>
public static class ListAccommodationsHandler
{
    public static async Task<Result<IReadOnlyList<AccommodationDto>>> Handle(
        ListAccommodations query,
        IAccommodationQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.ListAsync(cancellationToken));
    }
}
