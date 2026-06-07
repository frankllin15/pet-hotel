using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Domain.Tenants;
using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo Tenancy (schema próprio "tenancy", docs/04).
/// É a Unit of Work do módulo.
/// </summary>
public sealed class TenancyDbContext(DbContextOptions<TenancyDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "tenancy";

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
