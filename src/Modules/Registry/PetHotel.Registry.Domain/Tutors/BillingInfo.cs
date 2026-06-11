using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Dados de faturamento do tutor: documento fiscal (CPF/CNPJ), e-mail de cobrança e
/// endereço. Persistido como JSON dentro do agregado (owned), por isso é classe com
/// setters privados (mesmo padrão de EmergencyContact).
/// </summary>
public sealed class BillingInfo
{
    /// <summary>CPF ou CNPJ usado na cobrança/nota.</summary>
    public string Document { get; private set; } = null!;
    /// <summary>E-mail de cobrança quando diferente do e-mail principal. Opcional.</summary>
    public string? BillingEmail { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }

    private BillingInfo() { } // EF

    private BillingInfo(
        string document,
        string? billingEmail,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode)
    {
        Document = document;
        BillingEmail = billingEmail;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public static Result<BillingInfo> Create(
        string? document,
        string? billingEmail,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return Error.Validation("billing_info.document_required", "Documento (CPF/CNPJ) é obrigatório no faturamento.");
        }

        return new BillingInfo(
            document.Trim(),
            Normalize(billingEmail),
            Normalize(addressLine1),
            Normalize(addressLine2),
            Normalize(city),
            Normalize(state),
            Normalize(postalCode));
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
