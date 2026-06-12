namespace PetHotel.Registry.Application.Pets;

/// <summary>Projeção de leitura de um pet (docs/04).</summary>
public sealed record PetDto(
    Guid Id,
    Guid TutorId,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    string? Size,
    string? Sex,
    bool? Neutered,
    string? MicrochipCode,
    string? Notes,
    string? PhotoUrl,
    string? Sociability,
    string? Reactivity,
    string? Fear,
    string? Destructiveness,
    string? BehaviorNotes,
    FeedingRoutineDto? FeedingRoutine,
    DateTimeOffset CreatedAt);

/// <summary>Rotina alimentar do pet (leitura).</summary>
public sealed record FeedingRoutineDto(
    string FoodName,
    string? PortionSize,
    IReadOnlyList<TimeOnly> MealTimes,
    string? Restrictions,
    string FoodSource);
