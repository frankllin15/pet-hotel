using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Incidents;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Incidents.GetStayIncidents;

/// <summary>Delega para a porta de leitura (docs/04).</summary>
public static class GetStayIncidentsHandler
{
    public static async Task<Result<IReadOnlyList<IncidentDto>>> Handle(
        GetStayIncidents query,
        IIncidentQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.GetByContextAsync(query.ReservationId, cancellationToken));
    }
}
