namespace PetHotel.Health.Application.HealthRecords;

/// <summary>Projeção de leitura da ficha de saúde de um pet.</summary>
public sealed record PetHealthDto(
    Guid PetId,
    bool IsCleared,
    IReadOnlyList<string> Pendencies,
    IReadOnlyList<VaccinationDto> Vaccinations,
    IReadOnlyList<ParasiteTreatmentDto> ParasiteTreatments,
    VetContactDto? VetContact);

public sealed record VaccinationDto(
    Guid Id,
    string Type,
    DateOnly AppliedOn,
    DateOnly ExpiresOn,
    bool Valid,
    string? PhotoUrl);

/// <summary>Controle de parasitas (leitura). UpToDate é null quando não há próxima dose informada.</summary>
public sealed record ParasiteTreatmentDto(
    Guid Id,
    string Type,
    string? ProductName,
    DateOnly AppliedOn,
    DateOnly? NextDueOn,
    bool? UpToDate);

/// <summary>Veterinário particular (leitura).</summary>
public sealed record VetContactDto(string Name, string Phone, string? Clinic);
