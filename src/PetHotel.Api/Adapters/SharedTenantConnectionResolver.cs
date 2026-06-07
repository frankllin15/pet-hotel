using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;

namespace PetHotel.Api.Adapters;

/// <summary>
/// No MVP todos os tenants compartilham o mesmo banco (discriminador por TenantId).
/// A porta fica aberta para promover um tenant a banco dedicado depois (docs/04).
/// </summary>
public sealed class SharedTenantConnectionResolver(string connectionString) : ITenantConnectionResolver
{
    public string Resolve(TenantId tenant) => connectionString;
}
