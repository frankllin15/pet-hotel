using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Persistence;

/// <summary>
/// DbContext que conhece o tenant corrente, para o global query filter (docs/04).
/// Implementado por <see cref="ModuleDbContext"/> e pelo DbContext do Tenancy
/// (que herda de IdentityDbContext e por isso não estende ModuleDbContext).
/// </summary>
public interface ITenantScopedDbContext
{
    /// <summary>Tenant corrente, reavaliado pelo EF a cada consulta (parametrizado).</summary>
    TenantId CurrentTenant { get; }
}
