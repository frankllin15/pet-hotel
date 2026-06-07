namespace PetHotel.SharedKernel;

/// <summary>
/// Identificador de tenant. Conceito transversal (multi-tenancy por discriminador, docs/04).
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
