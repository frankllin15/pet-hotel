using Microsoft.EntityFrameworkCore;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;

namespace PetHotel.Health.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="HealthRecord"/>. Leituras já filtradas por tenant.</summary>
public sealed class HealthRecordRepository(HealthDbContext dbContext) : IHealthRecordRepository
{
    public Task<HealthRecord?> FindByPetAsync(PetReference pet, CancellationToken cancellationToken = default) =>
        dbContext.HealthRecords
            .Include(h => h.Vaccinations)
            .Include(h => h.ParasiteTreatments)
            .FirstOrDefaultAsync(h => h.Pet == pet, cancellationToken);

    public void Add(HealthRecord healthRecord) => dbContext.HealthRecords.Add(healthRecord);
}
