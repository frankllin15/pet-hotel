using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Persistence;

/// <summary>
/// Aplica o global query filter de tenant a toda entidade <see cref="IHasTenant"/>,
/// garantindo leitura isolada por tenant (docs/04).
/// </summary>
public static class TenantModelBuilderExtensions
{
    private static readonly MethodInfo SetFilterMethod = typeof(TenantModelBuilderExtensions)
        .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelBuilder ApplyTenantQueryFilter(this ModelBuilder modelBuilder, ModuleDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                SetFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(null, [modelBuilder, context]);
            }
        }

        return modelBuilder;
    }

    // O lambda captura o DbContext: o EF reavalia CurrentTenant a cada consulta.
    // Compara o TenantId inteiro (tem value converter) — comparar e.TenantId.Value
    // não é traduzível para SQL.
    private static void SetTenantFilter<TEntity>(ModelBuilder modelBuilder, ModuleDbContext context)
        where TEntity : class, IHasTenant
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == context.CurrentTenant);
    }
}
