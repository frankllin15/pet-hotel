using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Domain.Tasks;

namespace PetHotel.Operations.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="OperationalTask"/>.</summary>
public sealed class OperationalTaskRepository(OperationsDbContext dbContext) : IOperationalTaskRepository
{
    public void Add(OperationalTask task) => dbContext.Tasks.Add(task);

    public Task<OperationalTask?> FindAsync(OperationalTaskId id, CancellationToken cancellationToken = default) =>
        dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Remove(OperationalTask task) => dbContext.Tasks.Remove(task);
}
