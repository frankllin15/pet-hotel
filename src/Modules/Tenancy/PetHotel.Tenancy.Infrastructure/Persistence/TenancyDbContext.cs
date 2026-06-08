using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Configuration;
using PetHotel.Tenancy.Domain.Tenants;
using PetHotel.Tenancy.Infrastructure.Identity;

namespace PetHotel.Tenancy.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo Tenancy. Hospeda o ASP.NET Core Identity e as entidades de
/// domínio no mesmo contexto/schema ("tenancy"), de modo que o provisionamento
/// (tenant + config + admin) seja atômico em uma transação (docs/04).
/// Herda de IdentityDbContext, então implementa o tenant filter via
/// <see cref="ITenantScopedDbContext"/> em vez de estender ModuleDbContext.
/// </summary>
public sealed class TenancyDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, ITenantScopedDbContext
{
    public const string Schema = "tenancy";

    private readonly ITenantContext _tenantContext;

    public TenancyDbContext(DbContextOptions<TenancyDbContext> options, ITenantContext tenantContext)
        : base(options) => _tenantContext = tenantContext;

    public TenantId CurrentTenant => _tenantContext.HasTenant ? _tenantContext.Current : default;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantConfiguration> TenantConfigurations => Set<TenantConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // mapeamentos do Identity
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);

        // Tenant query filter para entidades IHasTenant (ex.: TenantConfiguration).
        modelBuilder.ApplyTenantQueryFilter(this);
    }
}
