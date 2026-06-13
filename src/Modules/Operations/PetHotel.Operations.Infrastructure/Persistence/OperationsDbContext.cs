using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Incidents;
using PetHotel.Operations.Domain.Medications;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>DbContext do módulo Operations (schema "operations", docs/04). É a Unit of Work.</summary>
public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "operations";

    public DbSet<CareLogEntry> CareLogEntries => Set<CareLogEntry>();
    public DbSet<MedicationAdministration> Medications => Set<MedicationAdministration>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OperationsDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
