using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>DbContext do módulo Registry (schema "registry", docs/04). É a Unit of Work.</summary>
public sealed class RegistryDbContext(DbContextOptions<RegistryDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "registry";

    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Pack> Packs => Set<Pack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistryDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
