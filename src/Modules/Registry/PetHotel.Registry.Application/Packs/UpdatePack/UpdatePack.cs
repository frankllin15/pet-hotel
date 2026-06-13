namespace PetHotel.Registry.Application.Packs.UpdatePack;

/// <summary>Edita uma matilha (nome, observações e composição) no tenant corrente.</summary>
public sealed record UpdatePack(Guid Id, string Name, string? Notes, IReadOnlyList<Guid> MemberPetIds);
