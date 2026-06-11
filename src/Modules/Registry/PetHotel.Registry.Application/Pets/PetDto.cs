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
    string? Sociability,
    string? Reactivity,
    string? Fear,
    string? Destructiveness,
    string? BehaviorNotes,
    DateTimeOffset CreatedAt);
