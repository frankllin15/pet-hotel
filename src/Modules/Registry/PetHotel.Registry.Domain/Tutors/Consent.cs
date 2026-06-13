using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Decisão de consentimento do tutor para uma finalidade (LGPD). Registra a escolha
/// (concedido/negado), o momento da decisão e a versão dos termos aceitos — exigências
/// de demonstração de consentimento. Persistido como JSON dentro do agregado (owned).
/// </summary>
public sealed class Consent
{
    public ConsentType Type { get; private set; }
    /// <summary>True = concedido; false = negado/revogado.</summary>
    public bool Granted { get; private set; }
    /// <summary>Momento em que a decisão atual foi tomada.</summary>
    public DateTimeOffset DecidedAt { get; private set; }
    /// <summary>Versão dos termos/política vigente quando a decisão foi registrada.</summary>
    public string TermsVersion { get; private set; } = null!;

    private Consent() { } // EF

    private Consent(ConsentType type, bool granted, DateTimeOffset decidedAt, string termsVersion)
    {
        Type = type;
        Granted = granted;
        DecidedAt = decidedAt;
        TermsVersion = termsVersion;
    }

    public static Result<Consent> Create(ConsentType type, bool granted, DateTimeOffset decidedAt, string? termsVersion)
    {
        if (!Enum.IsDefined(type))
        {
            return Error.Validation("consent.type_invalid", "Finalidade de consentimento inválida.");
        }

        if (string.IsNullOrWhiteSpace(termsVersion))
        {
            return Error.Validation("consent.terms_version_required", "Versão dos termos é obrigatória.");
        }

        return new Consent(type, granted, decidedAt, termsVersion.Trim());
    }
}
