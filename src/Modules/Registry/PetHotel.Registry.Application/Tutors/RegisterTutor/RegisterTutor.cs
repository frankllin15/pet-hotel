namespace PetHotel.Registry.Application.Tutors.RegisterTutor;

/// <summary>Cadastra um tutor no tenant corrente.</summary>
public sealed record RegisterTutor(
    string FullName,
    string Email,
    string Phone,
    IReadOnlyList<EmergencyContactInput>? EmergencyContacts = null,
    IReadOnlyList<AuthorizedPickupInput>? AuthorizedPickups = null);

/// <summary>Contato de emergência informado no cadastro.</summary>
public sealed record EmergencyContactInput(string Name, string Phone, string? Relationship);

/// <summary>Pessoa autorizada a retirar o pet informada no cadastro.</summary>
public sealed record AuthorizedPickupInput(string Name, string? Document);
