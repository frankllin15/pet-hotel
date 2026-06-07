using PetHotel.Tenancy.Application.Tenants;

namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>
/// Porta de leitura: projeta direto para DTO via EF (AsNoTracking), sem passar
/// pelo agregado (CQRS-lite, docs/04). Implementada na Infrastructure.
/// </summary>
public interface ITenantQueries
{
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
