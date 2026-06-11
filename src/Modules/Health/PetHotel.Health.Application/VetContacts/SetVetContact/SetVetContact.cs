namespace PetHotel.Health.Application.VetContacts.SetVetContact;

/// <summary>Define (ou substitui) o veterinário particular na ficha de um pet (cria a ficha se ainda não existir).</summary>
public sealed record SetVetContact(
    Guid PetId,
    string Name,
    string Phone,
    string? Clinic);
