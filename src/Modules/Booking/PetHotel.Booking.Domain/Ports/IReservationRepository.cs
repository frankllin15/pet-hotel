using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;

namespace PetHotel.Booking.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="Reservation"/>.</summary>
public interface IReservationRepository
{
    Task<Reservation?> FindAsync(ReservationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Quantas reservas DETÊM a acomodação (Confirmed ou CheckedIn) e conflitam com o período
    /// (opcionalmente ignorando uma reserva). Comparado à capacidade da acomodação para decidir
    /// overbooking — bloqueia só quando a ocupação atinge a capacidade.
    /// </summary>
    Task<int> CountActiveOverlapsAsync(
        AccommodationId accommodationId,
        DateRange period,
        ReservationId? excluding = null,
        CancellationToken cancellationToken = default);

    void Add(Reservation reservation);
}
