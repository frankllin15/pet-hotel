using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Tasks;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>Lado de leitura das tarefas operacionais (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class OperationalTaskQueries(OperationsDbContext dbContext) : IOperationalTaskQueries
{
    public async Task<IReadOnlyList<OperationalTaskDto>> ListByDateAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.Date == date)
            .OrderBy(t => t.Done) // pendentes primeiro
            .ThenBy(t => t.Category)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(t => new OperationalTaskDto(
                t.Id.Value, t.Title, t.Date, t.Category.ToString(), t.AssignedTo, t.Done, t.CompletedAt))
            .ToList();
    }
}
