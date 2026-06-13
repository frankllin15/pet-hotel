using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Domain.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Infrastructure.Persistence;

/// <summary>DbContext do módulo Notifications (schema "notifications", docs/04). É a Unit of Work.</summary>
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "notifications";

    public DbSet<OutboundMessage> OutboundMessages => Set<OutboundMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
