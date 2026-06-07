using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;

namespace PetHotel.Api.Adapters;

/// <summary>
/// Resolve o tenant a partir do claim do token (nunca da URL/query string, docs/04).
/// </summary>
public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public const string TenantClaim = "tenant_id";

    public bool HasTenant => TryRead(out _);

    public TenantId Current => TryRead(out var tenant)
        ? tenant
        : throw new InvalidOperationException("Nenhum tenant resolvido no contexto do request.");

    private bool TryRead(out TenantId tenant)
    {
        tenant = default;

        var value = httpContextAccessor.HttpContext?.User.FindFirst(TenantClaim)?.Value;
        if (Guid.TryParse(value, out var id))
        {
            tenant = new TenantId(id);
            return true;
        }

        return false;
    }
}
