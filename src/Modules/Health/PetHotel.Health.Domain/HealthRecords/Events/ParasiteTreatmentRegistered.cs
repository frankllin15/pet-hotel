using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords.Events;

/// <summary>Um controle de parasitas foi registrado na ficha de um pet.</summary>
public sealed record ParasiteTreatmentRegistered(
    HealthRecordId HealthRecordId,
    TenantId TenantId,
    PetReference Pet,
    ParasiteTreatmentType Type) : IDomainEvent;
