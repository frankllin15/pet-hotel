using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.GetOccupancy;

/// <summary>Projeta o calendário de ocupação direto para DTO (docs/04).</summary>
public static class GetOccupancyHandler
{
    public static async Task<Result<IReadOnlyList<OccupancyEntryDto>>> Handle(
        GetOccupancy query,
        IOccupancyQueries occupancy,
        CancellationToken cancellationToken)
    {
        if (query.To <= query.From)
        {
            return Error.Validation("occupancy.invalid_range", "O fim do intervalo deve ser após o início.");
        }

        var entries = await occupancy.GetAsync(query.From, query.To, cancellationToken);
        return Result.Success(entries);
    }
}
