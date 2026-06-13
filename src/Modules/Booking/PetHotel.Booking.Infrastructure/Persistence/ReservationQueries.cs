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

        return rows.Select(ToDto).ToList();
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == new ReservationId(id), cancellationToken);

        return reservation is null ? null : ToDto(reservation);
    }

    private static ReservationDto ToDto(Reservation r) =>
        new(
            r.Id.Value,
            r.Pet.Value,
            r.AccommodationId.Value,
            r.Period.Start,
            r.Period.End,
            r.Status.ToString(),
            r.CheckedInAt,
            r.CheckedOutAt,
            r.Period.Nights,
            r.DailyRate,
            r.TotalAmount,
            r.ArrivalState is { } a
                ? new ArrivalStateDto(a.WeightKg, a.Condition.ToString(), a.Observations)
                : null,
            r.ArrivalPhotoKeys.Select(k => $"/v1/files/{k}").ToList());
}
