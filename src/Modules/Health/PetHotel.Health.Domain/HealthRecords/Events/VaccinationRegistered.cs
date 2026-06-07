using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords.Events;

/// <summary>Uma vacinação foi registrada na ficha de um pet.</summary>
public sealed record VaccinationRegistered(
    HealthRecordId HealthRecordId,
    TenantId TenantId,
    PetReference Pet,
    VaccineType Type) : IDomainEvent;
