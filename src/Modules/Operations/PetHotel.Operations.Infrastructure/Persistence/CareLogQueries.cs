using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.CareLog;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>Lado de leitura do diário (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class CareLogQueries(OperationsDbContext dbContext) : ICareLogQueries
{
    public async Task<CursorPage<CareLogEntryDto>> GetByContextAsync(
        Guid contextId, Cursor? after, int limit, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CareLogEntries.AsNoTracking().Where(e => e.ContextId == contextId);

        // Keyset por OccurredAt desc (mais recentes primeiro); Id desc desempata.
        if (after is { } cursor)
        {
            query = query.Where(e => e.OccurredAt < cursor.Timestamp);
        }

        var rows = await query
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).Select(ToDto).ToList();
        var next = hasMore && items.Count > 0
            ? new Cursor(items[^1].OccurredAt, items[^1].Id).Encode()
            : null;

        return new CursorPage<CareLogEntryDto>(items, next);
    }

    private static CareLogEntryDto ToDto(CareLogEntry e) =>
        new(e.Id.Value, e.Pet.Value, e.Type.ToString(), e.Note, e.OccurredAt, e.CreatedBy,
            e.PhotoKeys.Select(k => $"/v1/files/{k}").ToList(), e.CreatedAt);
}
