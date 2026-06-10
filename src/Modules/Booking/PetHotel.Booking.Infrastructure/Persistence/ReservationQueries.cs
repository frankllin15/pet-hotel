using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>Lado de leitura de reservas (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class ReservationQueries(BookingDbContext dbContext) : IReservationQueries
{
    public async Task<IReadOnlyList<ReservationDto>> ListAsync(
        ReservationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reservations.AsNoTracking();

        if (status is { } value)
        {
            query = query.Where(r => r.Status == value);
        }

        var rows = await query
            .OrderBy(r => r.Period.Start)
            .ThenBy(r => r.Period.End)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new ReservationDto(
                r.Id.Value,
                r.Pet.Value,
                r.AccommodationId.Value,
                r.Period.Start,
                r.Period.End,
                r.Status.ToString(),
                r.CheckedInAt,
                r.CheckedOutAt))
            .ToList();
    }
}
