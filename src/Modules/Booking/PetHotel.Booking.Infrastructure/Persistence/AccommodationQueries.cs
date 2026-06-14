using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Accommodations;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>Lado de leitura de acomodações (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class AccommodationQueries(BookingDbContext dbContext) : IAccommodationQueries
{
    public async Task<IReadOnlyList<AccommodationDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Accommodations
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new AccommodationDto(a.Id.Value, a.Name, a.DailyRate, a.Capacity, a.Status.ToString()))
            .ToList();
    }
}
