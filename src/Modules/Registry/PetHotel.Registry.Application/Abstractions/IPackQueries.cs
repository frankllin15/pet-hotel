using PetHotel.Registry.Application.Packs;

namespace PetHotel.Registry.Application.Abstractions;

/// <summary>Porta de leitura de matilhas (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IPackQueries
{
    /// <summary>Matilhas do tenant (resumo), ordenadas por nome.</summary>
    Task<IReadOnlyList<PackSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Uma matilha por Id, com membros e alertas; null se não existir.</summary>
    Task<PackDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
