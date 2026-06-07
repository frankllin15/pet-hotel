using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Application.Pets.RegisterPet;

/// <summary>Cadastra um pet para um tutor existente no tenant corrente.</summary>
public sealed record RegisterPet(
    Guid TutorId,
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    string? Notes);
