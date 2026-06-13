using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Application.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Application.Reports.GetTutorReports;

/// <summary>Delega para a porta de leitura (docs/04).</summary>
public static class GetTutorReportsHandler
{
    public static async Task<Result<IReadOnlyList<OutboundMessageDto>>> Handle(
        GetTutorReports query,
        IOutboundMessageQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.GetByTutorAsync(query.TutorId, cancellationToken));
    }
}
