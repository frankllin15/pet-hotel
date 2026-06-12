namespace PetHotel.Registry.Application.Pets.SetPetPhoto;

/// <summary>
/// Associa (ou remove, com PhotoKey = null) a chave da foto ao pet. A gravação do
/// arquivo em si acontece no endpoint; aqui só carimbamos a referência no agregado.
/// </summary>
public sealed record SetPetPhoto(Guid PetId, string? PhotoKey);
