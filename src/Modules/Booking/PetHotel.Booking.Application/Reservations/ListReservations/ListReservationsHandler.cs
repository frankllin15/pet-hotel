using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.ListReservations;

/// <summary>Valida os filtros, normaliza a paginação e delega para a porta de leitura (docs/04).</summary>
public static class ListReservationsHandler
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public static async Task<Result<OffsetPage<ReservationDto>>> Handle(
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

        if (query.From is { } from && query.To is { } to && to < from)
        {
            return Error.Validation("reservation.invalid_range", "O fim do período deve ser igual ou após o início.");
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize,
        };

        var accommodationId = query.AccommodationId is { } id ? new AccommodationId(id) : (AccommodationId?)null;

        var result = await queries.ListAsync(
            status, accommodationId, query.From, query.To, page, pageSize, cancellationToken);

        return Result.Success(result);
    }
}
