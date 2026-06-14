using Microsoft.EntityFrameworkCore;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Medications;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>Lado de leitura das medicações (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class MedicationQueries(OperationsDbContext dbContext) : IMedicationQueries
{
    public async Task<IReadOnlyList<MedicationDto>> GetByContextAsync(
        Guid contextId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Medications
            .AsNoTracking()
            .Where(m => m.ContextId == contextId)
            .OrderByDescending(m => m.AdministeredAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(m => new MedicationDto(m.Id.Value, m.Drug, m.Dose, m.AdministeredAt, m.CreatedBy))
            .ToList();
    }

    public async Task<IReadOnlyList<DayMedicationDto>> GetByDateAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        // Janela [00:00, 24:00) em UTC do dia informado (AdministeredAt é timestamptz).
        var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = start.AddDays(1);

        var rows = await dbContext.Medications
            .AsNoTracking()
            .Where(m => m.AdministeredAt >= start && m.AdministeredAt < end)
            .OrderBy(m => m.AdministeredAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(m => new DayMedicationDto(m.Id.Value, m.ContextId, m.Drug, m.Dose, m.AdministeredAt))
            .ToList();
    }
}
