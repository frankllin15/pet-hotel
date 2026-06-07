using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Infrastructure.Persistence;

/// <summary>DbContext do módulo Health (schema "health", docs/04). É a Unit of Work.</summary>
public sealed class HealthDbContext(DbContextOptions<HealthDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "health";

    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HealthDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
