using Microsoft.EntityFrameworkCore;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Application.Vaccinations;

namespace PetHotel.Health.Infrastructure.Persistence;

/// <summary>
/// Alertas de vacina (AsNoTracking + query filter de tenant, docs/04). Por pet+tipo mantém só a
/// vacinação de maior validade (uma renovação não deixa a dose antiga disparando alerta).
/// </summary>
public sealed class VaccinationAlertQueries(HealthDbContext dbContext) : IVaccinationAlertQueries
{
    public async Task<IReadOnlyList<ExpiringVaccinationDto>> GetExpiringAsync(
        DateOnly asOf, int withinDays, CancellationToken cancellationToken = default)
    {
        var threshold = asOf.AddDays(withinDays);

        var records = await dbContext.HealthRecords
            .AsNoTracking()
            .Include(h => h.Vaccinations)
            .ToListAsync(cancellationToken);

        return records
            .SelectMany(r => r.Vaccinations.Select(v => (Pet: r.Pet.Value, v.Type, v.ExpiresOn)))
            .GroupBy(x => (x.Pet, x.Type))
            .Select(g => (g.Key.Pet, g.Key.Type, ExpiresOn: g.Max(x => x.ExpiresOn)))
            .Where(x => x.ExpiresOn <= threshold)
            .OrderBy(x => x.ExpiresOn)
            .Select(x => new ExpiringVaccinationDto(x.Pet, x.Type.ToString(), x.ExpiresOn))
            .ToList();
    }
}
