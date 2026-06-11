using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.HealthRecords.GetPetHealth;

/// <summary>
/// Lê a ficha do pet e calcula a aptidão via agregado (clearance é regra de domínio).
/// </summary>
public static class GetPetHealthHandler
{
    public static async Task<Result<PetHealthDto>> Handle(
        GetPetHealth query,
        IHealthRecordRepository healthRecords,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var record = await healthRecords.FindByPetAsync(new PetReference(query.PetId), cancellationToken);
        if (record is null)
        {
            return Error.NotFound("health.not_found", "Ficha de saúde não encontrada para o pet.");
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var clearance = record.GetClearance(today);

        var vaccinations = record.Vaccinations
            .Select(v => new VaccinationDto(v.Id.Value, v.Type.ToString(), v.AppliedOn, v.ExpiresOn, v.IsValidOn(today)))
            .ToList();

        var parasiteTreatments = record.ParasiteTreatments
            .OrderByDescending(t => t.AppliedOn)
            .Select(t => new ParasiteTreatmentDto(
                t.Id.Value, t.Type.ToString(), t.ProductName, t.AppliedOn, t.NextDueOn, t.IsUpToDateOn(today)))
            .ToList();

        var dto = new PetHealthDto(
            record.Pet.Value,
            clearance.IsCleared,
            clearance.Pendencies.Select(p => p.ToString()).ToList(),
            vaccinations,
            parasiteTreatments,
            record.VetContact is { } vet ? new VetContactDto(vet.Name, vet.Phone, vet.Clinic) : null);

        return dto;
    }
}
