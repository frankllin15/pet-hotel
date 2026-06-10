using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="Reservation"/>.</summary>
public interface IReservationRepository
{
    Task<Reservation?> FindAsync(ReservationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Existe reserva que ocupa a acomodação (confirmada, em estadia ou já encerrada — ou seja,
    /// qualquer estado que não seja apenas solicitado nem cancelado) e conflita com o período?
    /// (opcionalmente ignorando uma reserva).
    /// </summary>
    Task<bool> HasActiveOverlapAsync(
        AccommodationId accommodationId,
        DateRange period,
        ReservationId? excluding = null,
        CancellationToken cancellationToken = default);

    void Add(Reservation reservation);
}
