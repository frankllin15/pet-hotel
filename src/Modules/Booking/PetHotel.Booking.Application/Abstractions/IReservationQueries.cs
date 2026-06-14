using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Abstractions;

/// <summary>Porta de leitura de reservas (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IReservationQueries
{
    /// <summary>
    /// Reservas do tenant, ordenadas por check-in, paginadas por offset. Filtros opcionais:
    /// status, acomodação e janela de período (sobreposição com [from, to]).
    /// </summary>
    Task<OffsetPage<ReservationDto>> ListAsync(
        ReservationStatus? status,
        AccommodationId? accommodationId,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Uma reserva por Id no tenant corrente; null se não existir.</summary>
    Task<ReservationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pets que dividem a acomodação no período (reservas Confirmed/CheckedIn que sobrepõem),
    /// para o alerta de compatibilidade de compartilhamento. Ids distintos.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetActiveOverlapPetIdsAsync(
        Guid accommodationId, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken = default);
}
