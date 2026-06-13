using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>
/// Calendário de ocupação: reservas que ocupam a acomodação no intervalo (docs/04).
/// "Ocupa" aqui = confirmada, em estadia ou já encerrada (tudo menos apenas solicitada/cancelada),
/// para manter o histórico de check-in/check-out visível no calendário. NOTA: este critério é de
/// EXIBIÇÃO e é mais amplo que o de DISPONIBILIDADE — o bloqueio de overbooking
/// (<see cref="Repositories.ReservationRepository.HasActiveOverlapAsync"/>) conta só Confirmed/CheckedIn,
/// pois uma reserva encerrada (CheckedOut) já liberou a vaga.
/// </summary>
public sealed class OccupancyQueries(BookingDbContext dbContext) : IOccupancyQueries
{
    public async Task<IReadOnlyList<OccupancyEntryDto>> GetAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Status != ReservationStatus.Requested &&
                r.Status != ReservationStatus.Cancelled &&
                r.Period.Start < to && from < r.Period.End)
            .ToListAsync(cancellationToken);

        return reservations
            .Select(r => new OccupancyEntryDto(
                r.AccommodationId.Value, r.Id.Value, r.Pet.Value, r.Period.Start, r.Period.End))
            .ToList();
    }
}
