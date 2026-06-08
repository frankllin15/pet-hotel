using Microsoft.EntityFrameworkCore;
using PetHotel.Tenancy.Domain.Configuration;
using PetHotel.Tenancy.Domain.Ports;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Repositories;

/// <summary>Repositório da configuração do tenant (uma por tenant; query filter aplicado).</summary>
public sealed class TenantConfigurationRepository(TenancyDbContext dbContext) : ITenantConfigurationRepository
{
    public Task<TenantConfiguration?> GetForCurrentTenantAsync(CancellationToken cancellationToken = default) =>
        dbContext.TenantConfigurations.FirstOrDefaultAsync(cancellationToken);

    public void Add(TenantConfiguration configuration) => dbContext.TenantConfigurations.Add(configuration);
}
