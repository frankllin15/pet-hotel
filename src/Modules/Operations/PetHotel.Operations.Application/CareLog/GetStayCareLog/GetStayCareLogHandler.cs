using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.CareLog.GetStayCareLog;

/// <summary>Decodifica o cursor, normaliza o limite e delega para a porta de leitura (docs/04).</summary>
public static class GetStayCareLogHandler
{
    private const int MaxLimit = 100;

    public static async Task<Result<CursorPage<CareLogEntryDto>>> Handle(
        GetStayCareLog query,
        ICareLogQueries queries,
        CancellationToken cancellationToken)
    {
        Cursor? after = Cursor.TryDecode(query.Cursor, out var decoded) ? decoded : null;
        var limit = Math.Clamp(query.Limit, 1, MaxLimit);

        var page = await queries.GetByContextAsync(query.ReservationId, after, limit, cancellationToken);
        return Result.Success(page);
    }
}
