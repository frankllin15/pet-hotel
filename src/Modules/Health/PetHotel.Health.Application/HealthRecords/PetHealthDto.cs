namespace PetHotel.Health.Application.HealthRecords;

/// <summary>Projeção de leitura da ficha de saúde de um pet.</summary>
public sealed record PetHealthDto(
    Guid PetId,
    bool IsCleared,
    IReadOnlyList<string> Pendencies,
    IReadOnlyList<VaccinationDto> Vaccinations);

public sealed record VaccinationDto(
    Guid Id,
    string Type,
    DateOnly AppliedOn,
    DateOnly ExpiresOn,
    bool Valid);
