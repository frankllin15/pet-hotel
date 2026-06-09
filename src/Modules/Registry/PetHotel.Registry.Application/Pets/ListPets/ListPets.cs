namespace PetHotel.Registry.Application.Pets.ListPets;

/// <summary>
/// Lista pets do tenant corrente. <paramref name="Search"/> filtra por nome;
/// <paramref name="TutorId"/> restringe a um tutor; <paramref name="Cursor"/> é o
/// cursor opaco da página anterior (docs/04).
/// </summary>
public sealed record ListPets(
    string? Search = null,
    Guid? TutorId = null,
    string? Cursor = null,
    int Limit = 20);
