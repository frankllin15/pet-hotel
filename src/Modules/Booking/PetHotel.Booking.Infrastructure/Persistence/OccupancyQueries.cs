using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>Calendário de ocupação: reservas confirmadas que tocam o intervalo (docs/04).</summary>
public sealed class OccupancyQueries(BookingDbContext dbContext) : IOccupancyQueries
{
    public async Task<IReadOnlyList<OccupancyEntryDto>> GetAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Confirmed && r.Period.Start < to && from < r.Period.End)
            .ToListAsync(cancellationToken);

        return reservations
            .Select(r => new OccupancyEntryDto(
                r.AccommodationId.Value, r.Id.Value, r.Pet.Value, r.Period.Start, r.Period.End))
            .ToList();
    }
}
