namespace PetHotel.Health.Application.Vaccinations.SetVaccinationPhoto;

/// <summary>
/// Associa (ou remove, com PhotoKey = null) a foto da carteira a uma vacinação da ficha
/// do pet. A gravação do arquivo acontece no endpoint; aqui só carimbamos a referência.
/// </summary>
public sealed record SetVaccinationPhoto(Guid PetId, Guid VaccinationId, string? PhotoKey);
