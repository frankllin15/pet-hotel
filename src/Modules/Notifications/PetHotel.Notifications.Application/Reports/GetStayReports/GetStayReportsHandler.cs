using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Application.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Application.Reports.GetStayReports;

/// <summary>Delega para a porta de leitura (docs/04).</summary>
public static class GetStayReportsHandler
{
    public static async Task<Result<IReadOnlyList<OutboundMessageDto>>> Handle(
        GetStayReports query,
        IOutboundMessageQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.GetByReservationAsync(query.ReservationId, cancellationToken));
    }
}
