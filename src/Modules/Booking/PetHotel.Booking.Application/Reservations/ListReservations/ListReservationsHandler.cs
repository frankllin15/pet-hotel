using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.ListReservations;

/// <summary>Valida o filtro de status e delega para a porta de leitura (docs/04).</summary>
public static class ListReservationsHandler
{
    public static async Task<Result<IReadOnlyList<ReservationDto>>> Handle(
        ListReservations query,
        IReservationQueries queries,
        CancellationToken cancellationToken)
    {
        ReservationStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<ReservationStatus>(query.Status, ignoreCase: true, out var parsed))
            {
                return Error.Validation("reservation.invalid_status", "Status de reserva inválido.");
            }

            status = parsed;
        }

        return Result.Success(await queries.ListAsync(status, cancellationToken));
    }
}
