using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Ports;

namespace PetHotel.Operations.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="CareLogEntry"/>.</summary>
public sealed class CareLogRepository(OperationsDbContext dbContext) : ICareLogRepository
{
    public Task<CareLogEntry?> FindAsync(CareLogEntryId id, CancellationToken cancellationToken = default) =>
        dbContext.CareLogEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public void Add(CareLogEntry entry) => dbContext.CareLogEntries.Add(entry);
}
