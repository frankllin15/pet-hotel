namespace PetHotel.Registry.Application.Packs.CreatePack;

/// <summary>Cria uma matilha (grupo de pets) no tenant corrente.</summary>
public sealed record CreatePack(string Name, string? Notes, IReadOnlyList<Guid> MemberPetIds);
