using PetHotel.SharedKernel;
using PetHotel.Registry.Domain.Tutors.Events;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Tutor (dono dos pets). Agregado tenant-scoped e auditável (docs/03).
/// </summary>
public sealed class Tutor : AggregateRoot<TutorId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public string FullName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;
    /// <summary>Contatos de emergência (acionados se o tutor não for localizado).</summary>
    public List<EmergencyContact> EmergencyContacts { get; private set; } = [];
    /// <summary>Pessoas autorizadas a retirar o pet em nome do tutor.</summary>
    public List<AuthorizedPickup> AuthorizedPickups { get; private set; } = [];
    /// <summary>Dados de faturamento (opcional; preenchido quando o tutor informa).</summary>
    public BillingInfo? Billing { get; private set; }
    /// <summary>Decisões de consentimento LGPD (uma por finalidade já decidida).</summary>
    public List<Consent> Consents { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Tutor() { } // EF

    private Tutor(TutorId id, TenantId tenantId, string fullName, Email email, PhoneNumber phone) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    public static Result<Tutor> Register(
        TenantId tenantId,
        string? fullName,
        string? email,
        string? phone,
        IEnumerable<EmergencyContact>? emergencyContacts = null,
        IEnumerable<AuthorizedPickup>? authorizedPickups = null,
        BillingInfo? billing = null)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("tutor.tenant_required", "Tenant é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Error.Validation("tutor.name_required", "Nome do tutor é obrigatório.");
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
        {
            return phoneResult.Error;
        }

        var tutor = new Tutor(TutorId.New(), tenantId, fullName.Trim(), emailResult.Value, phoneResult.Value)
        {
            EmergencyContacts = emergencyContacts?.ToList() ?? [],
            AuthorizedPickups = authorizedPickups?.ToList() ?? [],
            Billing = billing,
        };
        tutor.Raise(new TutorRegistered(tutor.Id, tenantId, tutor.Email.Value));
        return tutor;
    }

    /// <summary>Edita os dados do tutor, inclusive as coleções de contatos/autorizados e o faturamento.</summary>
    public Result Update(
        string? fullName,
        string? email,
        string? phone,
        IEnumerable<EmergencyContact>? emergencyContacts = null,
        IEnumerable<AuthorizedPickup>? authorizedPickups = null,
        BillingInfo? billing = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Error.Validation("tutor.name_required", "Nome do tutor é obrigatório.");
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
        {
            return phoneResult.Error;
        }

        FullName = fullName.Trim();
        Email = emailResult.Value;
        Phone = phoneResult.Value;
        EmergencyContacts = emergencyContacts?.ToList() ?? [];
        AuthorizedPickups = authorizedPickups?.ToList() ?? [];
        Billing = billing;
        return Result.Success();
    }

    /// <summary>
    /// Registra a decisão de consentimento para uma finalidade (LGPD). Se a decisão for
    /// igual à atual, preserva o carimbo original (a data relevante é a da decisão real);
    /// se mudar (ou for nova), grava o novo estado com o momento e a versão dos termos.
    /// </summary>
    public Result SetConsent(ConsentType type, bool granted, DateTimeOffset now, string? termsVersion)
    {
        var existing = Consents.FirstOrDefault(c => c.Type == type);
        if (existing is not null && existing.Granted == granted)
        {
            return Result.Success();
        }

        var consent = Consent.Create(type, granted, now, termsVersion);
        if (consent.IsFailure)
        {
            return consent.Error;
        }

        if (existing is not null)
        {
            Consents.Remove(existing);
        }

        Consents.Add(consent.Value);
        return Result.Success();
    }

    /// <summary>Marca o tutor como excluído (levanta o evento; a remoção física é do repositório).</summary>
    public void Delete() => Raise(new TutorDeleted(Id, TenantId));
}
