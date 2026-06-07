using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Application.Vaccinations.RegisterVaccination;

/// <summary>Registra uma vacinação na ficha de um pet (cria a ficha se ainda não existir).</summary>
public sealed record RegisterVaccination(
    Guid PetId,
    VaccineType Type,
    DateOnly AppliedOn,
    DateOnly ExpiresOn);
