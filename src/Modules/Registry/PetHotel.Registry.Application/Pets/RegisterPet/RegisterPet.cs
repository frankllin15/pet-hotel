using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Application.Pets.RegisterPet;

/// <summary>Cadastra um pet para um tutor existente no tenant corrente.</summary>
public sealed record RegisterPet(
    Guid TutorId,
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSize? Size,
    Sex? Sex,
    bool? Neutered,
    string? MicrochipCode,
    string? Notes,
    FeedingRoutineInput? FeedingRoutine = null,
    IReadOnlyList<BelongingInput>? Belongings = null);

/// <summary>Rotina alimentar informada no cadastro/edição do pet.</summary>
public sealed record FeedingRoutineInput(
    string FoodName,
    string? PortionSize,
    IReadOnlyList<TimeOnly>? MealTimes,
    string? Restrictions,
    FoodSource FoodSource);

/// <summary>Pertence trazido pelo pet, informado no cadastro/edição.</summary>
public sealed record BelongingInput(
    string Name,
    int Quantity,
    string? Notes);
