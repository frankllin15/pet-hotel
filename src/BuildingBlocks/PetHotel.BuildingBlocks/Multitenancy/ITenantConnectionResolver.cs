using PetHotel.SharedKernel;

namespace PetHotel.BuildingBlocks.Multitenancy;

/// <summary>
/// Resolve a connection string por tenant. Mantém aberta a evolução para
/// banco dedicado sem mudar o domínio (docs/04). No MVP devolve a string compartilhada.
/// </summary>
public interface ITenantConnectionResolver
{
    string Resolve(TenantId tenant);
}
