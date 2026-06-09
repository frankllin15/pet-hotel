using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Reservation"/> (leituras filtradas por tenant).</summary>
public sealed class ReservationRepository(BookingDbContext dbContext) : IReservationRepository
{
    public Task<Reservation?> FindAsync(ReservationId id, CancellationToken cancellationToken = default) =>
        dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> HasConfirmedOverlapAsync(
        AccommodationId accommodationId,
        DateRange period,
        ReservationId? excluding = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reservations.Where(r =>
            r.AccommodationId == accommodationId &&
            r.Status == ReservationStatus.Confirmed &&
            r.Period.Start < period.End &&
            period.Start < r.Period.End);

        if (excluding is { } exclude)
        {
            query = query.Where(r => r.Id != exclude);
        }

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Reservation reservation) => dbContext.Reservations.Add(reservation);
}
