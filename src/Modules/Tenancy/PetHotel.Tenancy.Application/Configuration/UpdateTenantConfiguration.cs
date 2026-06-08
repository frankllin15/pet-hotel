namespace PetHotel.Tenancy.Application.Configuration;

/// <summary>Wizard de setup: define tipos de acomodação, vacinas obrigatórias e horários.</summary>
public sealed record UpdateTenantConfiguration(
    IReadOnlyList<AccommodationTypeInput> AccommodationTypes,
    IReadOnlyList<string> RequiredVaccines,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime);

public sealed record AccommodationTypeInput(string Name, int Capacity, decimal DailyPrice);
