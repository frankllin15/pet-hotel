using PetHotel.Registry.Application.Tutors.RegisterTutor;

namespace PetHotel.Registry.Application.Tutors.UpdateTutor;

/// <summary>Edita os dados de um tutor existente no tenant corrente.</summary>
public sealed record UpdateTutor(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    IReadOnlyList<EmergencyContactInput>? EmergencyContacts = null,
    IReadOnlyList<AuthorizedPickupInput>? AuthorizedPickups = null);
