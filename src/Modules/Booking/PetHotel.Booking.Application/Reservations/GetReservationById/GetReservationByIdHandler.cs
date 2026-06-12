using PetHotel.Booking.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.GetReservationById;

/// <summary>Projeta direto para DTO via porta de leitura (docs/04).</summary>
public static class GetReservationByIdHandler
{
    public static async Task<Result<ReservationDto>> Handle(
        GetReservationById query,
        IReservationQueries queries,
        CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(query.Id, cancellationToken);

        return dto is null
            ? Error.NotFound("reservation.not_found", "Reserva não encontrada.")
            : dto;
    }
}
