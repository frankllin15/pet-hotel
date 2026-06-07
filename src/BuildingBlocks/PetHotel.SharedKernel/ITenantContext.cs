namespace PetHotel.SharedKernel;

/// <summary>
/// Tenant corrente do request, populado a partir do token de autenticação.
/// Nunca lido de URL/query string (docs/04). Porta pura usada por Application e Infrastructure.
/// </summary>
public interface ITenantContext
{
    /// <summary>Tenant corrente. Lança se não houver tenant resolvido.</summary>
    TenantId Current { get; }

    /// <summary>Indica se há tenant resolvido no escopo atual.</summary>
    bool HasTenant { get; }
}
