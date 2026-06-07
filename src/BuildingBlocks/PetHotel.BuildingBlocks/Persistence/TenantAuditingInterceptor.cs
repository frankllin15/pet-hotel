using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PetHotel.BuildingBlocks.Auditing;
using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Persistence;

/// <summary>
/// Carimba <c>TenantId</c> em toda inserção de <see cref="IHasTenant"/> e preenche
/// auditoria de <see cref="IAuditable"/>, evitando vazamento por esquecimento (docs/04).
/// </summary>
public sealed class TenantAuditingInterceptor(
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var user = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            StampTenant(entry);
            StampAudit(entry, now, user);
        }
    }

    private void StampTenant(EntityEntry entry)
    {
        if (entry.State is not EntityState.Added || entry.Entity is not IHasTenant)
        {
            return;
        }

        var property = entry.Property(nameof(IHasTenant.TenantId));
        var current = (TenantId)(property.CurrentValue ?? default(TenantId));

        // Só carimba quando ainda não definido — respeita quem já setou no agregado.
        if (current == default && tenantContext.HasTenant)
        {
            property.CurrentValue = tenantContext.Current;
        }
    }

    private static void StampAudit(EntityEntry entry, DateTimeOffset now, string? user)
    {
        if (entry.Entity is not IAuditable)
        {
            return;
        }

        switch (entry.State)
        {
            case EntityState.Added:
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = user;
                break;
            case EntityState.Modified:
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = user;
                break;
        }
    }
}
