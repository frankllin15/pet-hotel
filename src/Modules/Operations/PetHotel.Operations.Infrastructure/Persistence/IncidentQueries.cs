using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Incidents;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>Lado de leitura dos incidentes (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class IncidentQueries(OperationsDbContext dbContext) : IIncidentQueries
{
    public async Task<IReadOnlyList<IncidentDto>> GetByContextAsync(
        Guid contextId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Incidents
            .AsNoTracking()
            .Where(i => i.ContextId == contextId)
            .OrderByDescending(i => i.OccurredAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(i => new IncidentDto(i.Id.Value, i.Severity.ToString(), i.Description, i.OccurredAt, i.CreatedBy, i.CreatedAt))
            .ToList();
    }
}
