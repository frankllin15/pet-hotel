namespace PetHotel.Registry.Application.Contracts;

/// <summary>
/// Compatibilidade comportamental de um pet, exposta a outros módulos. Livre de tipos internos
/// do Registry — as flags são nomes (ex.: "Reactive", "LowSociability") (docs/01).
/// </summary>
public sealed record PetCompatibility(Guid PetId, string Name, IReadOnlyList<string> Flags);
