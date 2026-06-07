using Microsoft.EntityFrameworkCore;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Tenants;

namespace PetHotel.Tenancy.Infrastructure.Persistence;

/// <summary>Lado de leitura do Tenancy (AsNoTracking, docs/04).</summary>
public sealed class TenantQueries(TenancyDbContext dbContext) : ITenantQueries
{
    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == new TenantId(id), cancellationToken);

        return tenant is null
            ? null
            : new TenantDto(tenant.Id.Value, tenant.Name, tenant.Slug.Value, tenant.Status.ToString(), tenant.CreatedAt);
    }
}
