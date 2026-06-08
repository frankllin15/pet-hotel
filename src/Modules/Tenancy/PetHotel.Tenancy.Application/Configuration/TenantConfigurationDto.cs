namespace PetHotel.Tenancy.Application.Configuration;

/// <summary>Projeção de leitura da configuração do tenant.</summary>
public sealed record TenantConfigurationDto(
    IReadOnlyList<AccommodationTypeDto> AccommodationTypes,
    IReadOnlyList<string> RequiredVaccines,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    bool SetupCompleted);

public sealed record AccommodationTypeDto(string Name, int Capacity, decimal DailyPrice);
