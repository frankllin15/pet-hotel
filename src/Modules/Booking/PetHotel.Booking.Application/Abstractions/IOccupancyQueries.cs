using PetHotel.Booking.Application.Reservations;

namespace PetHotel.Booking.Application.Abstractions;

/// <summary>Porta de leitura do calendário de ocupação (AsNoTracking, docs/04).</summary>
public interface IOccupancyQueries
{
    Task<IReadOnlyList<OccupancyEntryDto>> GetAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
