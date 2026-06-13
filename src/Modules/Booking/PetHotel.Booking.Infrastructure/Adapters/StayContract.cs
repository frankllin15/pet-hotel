using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Contracts;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;

namespace PetHotel.Booking.Infrastructure.Adapters;

/// <summary>Implementação do contrato público de estadia sobre o <see cref="BookingDbContext"/> (docs/01).</summary>
public sealed class StayContract(BookingDbContext dbContext) : IStayContract
{
    public async Task<StayInfo?> FindStayAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == new ReservationId(reservationId), cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        // Pet presente quando o check-in foi feito (em estadia ou já encerrada).
        var arrived = reservation.Status is ReservationStatus.CheckedIn or ReservationStatus.CheckedOut;
        return new StayInfo(reservation.Id.Value, reservation.Pet.Value, arrived);
    }
}
