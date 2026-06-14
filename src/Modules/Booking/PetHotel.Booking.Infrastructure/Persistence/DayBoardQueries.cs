using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>
/// Painel do dia (AsNoTracking + query filter de tenant, docs/04). Carrega as reservas que
/// tocam o dia e as acomodações disponíveis e consolida chegadas/saídas/estadia + ocupação.
/// </summary>
public sealed class DayBoardQueries(BookingDbContext dbContext) : IDayBoardQueries
{
    public async Task<DayBoardDto> GetAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var next = date.AddDays(1);

        // Reservas relevantes ao dia: chegando, saindo ou cobrindo o dia (meio-aberto [Start, End)).
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(r =>
                (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedIn) &&
                r.Period.Start < next && date < r.Period.End)
            .OrderBy(r => r.Period.Start)
            .ThenBy(r => r.Period.End)
            .ToListAsync(cancellationToken);

        // Chegadas previstas: confirmadas cujo check-in é hoje (ainda não chegaram).
        var arrivals = reservations
            .Where(r => r.Status == ReservationStatus.Confirmed && r.Period.Start == date)
            .Select(ReservationQueries.ToDto)
            .ToList();

        // Saídas previstas: em estadia cujo check-out é hoje.
        var departures = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.CheckedIn && r.Period.End == date)
            .OrderBy(r => r.Period.End)
            .ToListAsync(cancellationToken);

        var inHouse = reservations
            .Where(r => r.Status == ReservationStatus.CheckedIn)
            .Select(ReservationQueries.ToDto)
            .ToList();

        // Ocupação por vaga: vagas ocupadas (reservas ativas cobrindo o dia) sobre a
        // capacidade total das acomodações disponíveis.
        var occupiedSlots = reservations.Count;

        var totalSlots = await dbContext.Accommodations
            .AsNoTracking()
            .Where(a => a.Status == AccommodationStatus.Available)
            .SumAsync(a => a.Capacity, cancellationToken);

        return new DayBoardDto(
            arrivals,
            departures.Select(ReservationQueries.ToDto).ToList(),
            inHouse,
            occupiedSlots,
            totalSlots);
    }
}
