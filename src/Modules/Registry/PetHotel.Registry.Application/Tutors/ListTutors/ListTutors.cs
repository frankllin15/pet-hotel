namespace PetHotel.Registry.Application.Tutors.ListTutors;

/// <summary>
/// Lista tutores do tenant corrente. <paramref name="Search"/> filtra por nome;
/// <paramref name="Cursor"/> é o cursor opaco da página anterior (docs/04).
/// </summary>
public sealed record ListTutors(string? Search = null, string? Cursor = null, int Limit = 20);
