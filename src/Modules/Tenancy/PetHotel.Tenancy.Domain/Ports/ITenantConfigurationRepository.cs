using PetHotel.Tenancy.Domain.Configuration;

namespace PetHotel.Tenancy.Domain.Ports;

/// <summary>Porta de saída para a configuração do tenant corrente.</summary>
public interface ITenantConfigurationRepository
{
    /// <summary>Configuração do tenant corrente (query filter aplicado).</summary>
    Task<TenantConfiguration?> GetForCurrentTenantAsync(CancellationToken cancellationToken = default);

    void Add(TenantConfiguration configuration);
}
