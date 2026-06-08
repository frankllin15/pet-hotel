using Microsoft.EntityFrameworkCore;
using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Persistence;

/// <summary>
/// Base de DbContext por módulo (docs/04). Expõe o tenant corrente para o global
/// query filter e aplica o filtro a toda entidade <see cref="IHasTenant"/>.
/// </summary>
public abstract class ModuleDbContext : DbContext, ITenantScopedDbContext
{
    private readonly ITenantContext _tenantContext;

    protected ModuleDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options) => _tenantContext = tenantContext;

    /// <summary>
    /// Tenant corrente, lido pelo EF a cada consulta (parametrizado), de modo que
    /// um mesmo modelo cacheado sirva requests de tenants diferentes.
    /// </summary>
    public TenantId CurrentTenant => _tenantContext.HasTenant ? _tenantContext.Current : default;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyTenantQueryFilter(this);
    }
}
