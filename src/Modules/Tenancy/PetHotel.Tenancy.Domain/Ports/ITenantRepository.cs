using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Tenants;

namespace PetHotel.Tenancy.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="Tenant"/>.</summary>
public interface ITenantRepository
{
    Task<Tenant?> FindAsync(TenantId id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default);
    void Add(Tenant tenant);
}
