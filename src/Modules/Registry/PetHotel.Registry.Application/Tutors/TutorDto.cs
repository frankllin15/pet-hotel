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
    IReadOnlyList<ConsentDto> Consents,
    DateTimeOffset CreatedAt);

/// <summary>Decisão de consentimento LGPD (leitura).</summary>
public sealed record ConsentDto(
    string Type,
    bool Granted,
    DateTimeOffset DecidedAt,
    string TermsVersion);

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
