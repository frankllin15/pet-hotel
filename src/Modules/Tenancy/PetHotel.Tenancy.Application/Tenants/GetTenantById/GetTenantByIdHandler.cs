using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Application.Tenants.GetTenantById;

/// <summary>Projeta direto para DTO via porta de leitura (não passa pelo agregado, docs/04).</summary>
public static class GetTenantByIdHandler
{
    public static async Task<Result<TenantDto>> Handle(
        GetTenantById query,
        ITenantQueries queries,
        CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(query.Id, cancellationToken);

        return dto is null
            ? Error.NotFound("tenant.not_found", "Tenant não encontrado.")
            : dto;
    }
}
