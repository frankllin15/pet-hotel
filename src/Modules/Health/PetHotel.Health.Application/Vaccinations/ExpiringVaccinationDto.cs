namespace PetHotel.Health.Application.Vaccinations;

/// <summary>
/// Alerta de vacina vencida ou a vencer (painel). Por pet+tipo considera só a vacinação de
/// maior validade (renovações não disparam alerta da dose antiga).
/// </summary>
public sealed record ExpiringVaccinationDto(
    Guid PetId,
    string VaccineType,
    DateOnly ExpiresOn);
