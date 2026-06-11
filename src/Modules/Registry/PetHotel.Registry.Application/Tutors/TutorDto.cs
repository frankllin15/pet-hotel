namespace PetHotel.Registry.Application.Tutors;

/// <summary>Projeção de leitura de um tutor (docs/04).</summary>
public sealed record TutorDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    IReadOnlyList<EmergencyContactDto> EmergencyContacts,
    IReadOnlyList<AuthorizedPickupDto> AuthorizedPickups,
    BillingInfoDto? Billing,
    DateTimeOffset CreatedAt);

/// <summary>Dados de faturamento (leitura).</summary>
public sealed record BillingInfoDto(
    string Document,
    string? BillingEmail,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode);

/// <summary>Contato de emergência (leitura).</summary>
public sealed record EmergencyContactDto(string Name, string Phone, string? Relationship);

/// <summary>Pessoa autorizada a retirar o pet (leitura).</summary>
public sealed record AuthorizedPickupDto(string Name, string? Document);
