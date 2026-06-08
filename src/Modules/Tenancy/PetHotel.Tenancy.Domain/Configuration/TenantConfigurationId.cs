namespace PetHotel.Tenancy.Domain.Configuration;

/// <summary>Identificador tipado da configuração do tenant.</summary>
public readonly record struct TenantConfigurationId(Guid Value)
{
    public static TenantConfigurationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
