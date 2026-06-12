using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Application.Vaccinations.UpdateVaccination;

/// <summary>Edita uma vacinação existente na ficha de um pet.</summary>
public sealed record UpdateVaccination(
    Guid PetId,
    Guid VaccinationId,
    VaccineType Type,
    DateOnly AppliedOn,
    DateOnly ExpiresOn);
