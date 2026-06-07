using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Multitenancy;

/// <summary>
/// Entidade pertencente a um tenant. Recebe carimbo automático na escrita
/// e global query filter na leitura (docs/04).
/// </summary>
public interface IHasTenant
{
    TenantId TenantId { get; }
}
