using Microsoft.EntityFrameworkCore;
using PetHotel.Health.Application.Contracts;
using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Infrastructure.Persistence;

/// <summary>
/// Implementação do contrato público de clearance (docs/01). Calcula a aptidão via
/// agregado, no tenant corrente (query filter aplicado).
/// </summary>
public sealed class HealthClearanceContract(HealthDbContext dbContext) : IHealthClearanceContract
{
    public async Task<PetHealthClearance> GetClearanceAsync(
        Guid petId,
        DateOnly asOf,
        CancellationToken cancellationToken = default)
    {
        var pet = new PetReference(petId);

        var record = await dbContext.HealthRecords
            .AsNoTracking()
            .Include(h => h.Vaccinations)
            .FirstOrDefaultAsync(h => h.Pet == pet, cancellationToken);

        if (record is null)
        {
            // Sem ficha: nenhuma vacina obrigatória cumprida.
            return new PetHealthClearance(
                petId,
                false,
                HealthRecord.RequiredVaccines.Select(v => v.ToString()).ToList());
        }

        var clearance = record.GetClearance(asOf);
        return new PetHealthClearance(
            petId,
            clearance.IsCleared,
            clearance.Pendencies.Select(p => p.ToString()).ToList());
    }
}
