using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>Lado de leitura de reservas (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class ReservationQueries(BookingDbContext dbContext) : IReservationQueries
{
    public async Task<OffsetPage<ReservationDto>> ListAsync(
        ReservationStatus? status,
        AccommodationId? accommodationId,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reservations.AsNoTracking();

        if (status is { } value)
        {
            query = query.Where(r => r.Status == value);
        }

        if (accommodationId is { } accId)
        {
            query = query.Where(r => r.AccommodationId == accId);
        }

        // Janela de período: reservas que se sobrepõem a [from, to] (limites inclusivos por dia).
        if (from is { } start)
        {
            query = query.Where(r => start <= r.Period.End);
        }

        if (to is { } end)
        {
            query = query.Where(r => r.Period.Start <= end);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(r => r.Period.Start)
            .ThenBy(r => r.Period.End)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new OffsetPage<ReservationDto>(rows.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == new ReservationId(id), cancellationToken);

        return reservation is null ? null : ToDto(reservation);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveOverlapPetIdsAsync(
        Guid accommodationId, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken = default)
    {
        var accId = new AccommodationId(accommodationId);

        // Mesmo critério de posse de vaga do overbooking: Confirmed/CheckedIn que sobrepõem [checkIn, checkOut).
        var petIds = await dbContext.Reservations
            .AsNoTracking()
            .Where(r =>
                r.AccommodationId == accId &&
                (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedIn) &&
                r.Period.Start < checkOut &&
                checkIn < r.Period.End)
            .Select(r => r.Pet.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return petIds;
    }

    internal static ReservationDto ToDto(Reservation r) =>
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
