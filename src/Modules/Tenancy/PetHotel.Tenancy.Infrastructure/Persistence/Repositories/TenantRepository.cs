using Microsoft.EntityFrameworkCore;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Domain.Tenants;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Tenant"/> (só operações de agregado, docs/04).</summary>
public sealed class TenantRepository(TenancyDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> FindAsync(TenantId id, CancellationToken cancellationToken = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default) =>
        dbContext.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken);

    public void Add(Tenant tenant) => dbContext.Tenants.Add(tenant);
}
