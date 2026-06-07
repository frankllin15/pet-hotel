using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Persistence;

/// <summary>
/// Aplica o global query filter de tenant a toda entidade <see cref="IHasTenant"/>,
/// garantindo leitura isolada por tenant (docs/04).
/// </summary>
public static class TenantModelBuilderExtensions
{
    public static ModelBuilder ApplyTenantQueryFilter(this ModelBuilder modelBuilder, ITenantContext tenantContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // e => e.TenantId.Value == tenantContext.Current.Value
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var entityTenantValue = Expression.Property(
                Expression.Property(parameter, nameof(IHasTenant.TenantId)),
                nameof(TenantId.Value));
            var currentTenantValue = Expression.Property(
                Expression.Property(Expression.Constant(tenantContext), nameof(ITenantContext.Current)),
                nameof(TenantId.Value));

            var filter = Expression.Lambda(Expression.Equal(entityTenantValue, currentTenantValue), parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        return modelBuilder;
    }
}
