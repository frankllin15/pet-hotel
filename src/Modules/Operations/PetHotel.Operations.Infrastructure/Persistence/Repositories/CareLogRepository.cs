using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Ports;

namespace PetHotel.Operations.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="CareLogEntry"/>.</summary>
public sealed class CareLogRepository(OperationsDbContext dbContext) : ICareLogRepository
{
    public void Add(CareLogEntry entry) => dbContext.CareLogEntries.Add(entry);
}
