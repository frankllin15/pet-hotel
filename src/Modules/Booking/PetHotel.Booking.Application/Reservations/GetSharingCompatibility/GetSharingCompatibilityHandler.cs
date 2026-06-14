using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;
using Wolverine.Attributes;

namespace PetHotel.Booking.Application.Reservations.GetSharingCompatibility;

/// <summary>
/// Monta o alerta de compatibilidade do compartilhamento: descobre os co-ocupantes da vaga no
/// período (lado de leitura do Booking) e consulta a avaliação comportamental deles + a do pet
/// candidato (gateway → contrato do Registry). Retorna os pets com sinais de atenção.
/// </summary>
/// <remarks>
/// Fora do Wolverine (<see cref="WolverineIgnoreAttribute"/>): toca o BookingDbContext (leitura)
/// e o gateway do Registry — mesmo idioma do ConfirmReservation. Invocado direto via DI no endpoint.
/// </remarks>
[WolverineIgnore]
public static class GetSharingCompatibilityHandler
{
    public static async Task<Result<SharingCompatibilityDto>> Handle(
        GetSharingCompatibility query,
        IReservationQueries reservations,
        IPetCompatibilityGateway compatibility,
        CancellationToken cancellationToken)
    {
        var period = DateRange.Create(query.CheckIn, query.CheckOut);
        if (period.IsFailure)
        {
            return period.Error;
        }

        var coOccupants = (await reservations.GetActiveOverlapPetIdsAsync(
                query.AccommodationId, query.CheckIn, query.CheckOut, cancellationToken))
            .Where(id => id != query.PetId)
            .ToList();

        if (coOccupants.Count == 0)
        {
            return new SharingCompatibilityDto(Shared: false, []);
        }

        // Avalia o grupo todo (co-ocupantes + candidato) e mantém só quem tem sinais de atenção.
        var group = coOccupants.Append(query.PetId).ToList();
        var infos = await compatibility.GetCompatibilityAsync(group, cancellationToken);

        var conflicts = infos
            .Where(p => p.Flags.Count > 0)
            .Select(p => new PetCompatibilityDto(p.PetId, p.Name, p.Flags))
            .ToList();

        return new SharingCompatibilityDto(Shared: true, conflicts);
    }
}
