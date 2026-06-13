using PetHotel.Operations.Domain.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.Medications;

/// <summary>
/// Registro auditável de administração de medicamento durante a estadia ("quem/quando/dose").
/// Quem aplicou = auditoria (CreatedBy). Vinculado ao contexto de presença (estadia/creche).
/// </summary>
public sealed class MedicationAdministration : AggregateRoot<MedicationAdministrationId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public PetReference Pet { get; private set; }
    public CareContextType ContextType { get; private set; }
    public Guid ContextId { get; private set; }
    /// <summary>Medicamento aplicado.</summary>
    public string Drug { get; private set; } = null!;
    /// <summary>Dose aplicada (texto livre, ex.: "1 comprimido", "2 ml").</summary>
    public string Dose { get; private set; } = null!;
    /// <summary>Momento da aplicação.</summary>
    public DateTimeOffset AdministeredAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private MedicationAdministration() { } // EF

    private MedicationAdministration(
        MedicationAdministrationId id, TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        string drug, string dose, DateTimeOffset administeredAt) : base(id)
    {
        TenantId = tenantId;
        Pet = pet;
        ContextType = contextType;
        ContextId = contextId;
        Drug = drug;
        Dose = dose;
        AdministeredAt = administeredAt;
    }

    public static Result<MedicationAdministration> Administer(
        TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        string? drug, string? dose, DateTimeOffset administeredAt, DateTimeOffset now)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("medication.tenant_required", "Tenant é obrigatório.");
        }

        if (pet.Value == Guid.Empty || contextId == Guid.Empty)
        {
            return Error.Validation("medication.context_required", "Estadia é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(drug))
        {
            return Error.Validation("medication.drug_required", "Medicamento é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dose))
        {
            return Error.Validation("medication.dose_required", "Dose é obrigatória.");
        }

        if (administeredAt > now)
        {
            return Error.Validation("medication.administered_in_future", "A aplicação não pode estar no futuro.");
        }

        return new MedicationAdministration(
            MedicationAdministrationId.New(), tenantId, pet, contextType, contextId, drug.Trim(), dose.Trim(), administeredAt);
    }
}
