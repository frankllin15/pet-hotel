using PetHotel.Health.Domain.HealthRecords.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Ficha de saúde de um pet (uma por pet). Raiz de agregado tenant-scoped e auditável.
/// Centraliza as vacinações e calcula a aptidão sanitária (clearance) (docs/03).
/// </summary>
public sealed class HealthRecord : AggregateRoot<HealthRecordId>, IHasTenant, IAuditable
{
    /// <summary>
    /// Vacinas obrigatórias para hospedagem. Simplificação do MVP — a regra real
    /// viria da TenantConfiguration (docs/03).
    /// </summary>
    public static readonly IReadOnlyList<VaccineType> RequiredVaccines = [VaccineType.Rabies];

    private readonly List<Vaccination> _vaccinations = [];

    public TenantId TenantId { get; private set; }
    public PetReference Pet { get; private set; }
    public IReadOnlyCollection<Vaccination> Vaccinations => _vaccinations.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private HealthRecord() { } // EF

    private HealthRecord(HealthRecordId id, TenantId tenantId, PetReference pet) : base(id)
    {
        TenantId = tenantId;
        Pet = pet;
    }

    public static Result<HealthRecord> Open(TenantId tenantId, PetReference pet)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("health.tenant_required", "Tenant é obrigatório.");
        }

        if (pet.Value == Guid.Empty)
        {
            return Error.Validation("health.pet_required", "Pet é obrigatório.");
        }

        return new HealthRecord(HealthRecordId.New(), tenantId, pet);
    }

    public Result<VaccinationId> AddVaccination(VaccineType type, DateOnly appliedOn, DateOnly expiresOn, DateOnly today)
    {
        if (appliedOn > today)
        {
            return Error.Validation("vaccination.applied_future", "A aplicação não pode ser no futuro.");
        }

        if (expiresOn <= appliedOn)
        {
            return Error.Validation("vaccination.invalid_validity", "A validade deve ser posterior à aplicação.");
        }

        var vaccination = new Vaccination(VaccinationId.New(), type, appliedOn, expiresOn);
        _vaccinations.Add(vaccination);
        Raise(new VaccinationRegistered(Id, TenantId, Pet, type));
        return vaccination.Id;
    }

    /// <summary>Avalia a aptidão sanitária na data informada.</summary>
    public HealthClearance GetClearance(DateOnly asOf)
    {
        var pendencies = RequiredVaccines
            .Where(required => !_vaccinations.Any(v => v.Type == required && v.IsValidOn(asOf)))
            .ToList();

        return new HealthClearance(pendencies.Count == 0, pendencies);
    }
}
