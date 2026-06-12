using PetHotel.Registry.Application.Pets.RegisterPet;
using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Application.Pets.UpdatePet;

/// <summary>Edita os dados de um pet existente no tenant corrente.</summary>
public sealed record UpdatePet(
    Guid Id,
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSize? Size,
    Sex? Sex,
    bool? Neutered,
    string? MicrochipCode,
    string? Notes,
    BehaviorLevel? Sociability = null,
    BehaviorLevel? Reactivity = null,
    BehaviorLevel? Fear = null,
    BehaviorLevel? Destructiveness = null,
    string? BehaviorNotes = null,
    FeedingRoutineInput? FeedingRoutine = null,
    IReadOnlyList<BelongingInput>? Belongings = null);
