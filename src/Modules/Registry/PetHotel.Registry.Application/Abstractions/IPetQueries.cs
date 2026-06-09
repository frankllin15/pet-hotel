using PetHotel.Registry.Application.Pets;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Abstractions;

/// <summary>Porta de leitura de pets (AsNoTracking, docs/04).</summary>
public interface IPetQueries
{
    Task<PetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista pets do tenant corrente, paginada por cursor. <paramref name="tutorId"/>
    /// opcional restringe aos pets de um tutor (docs/04).
    /// </summary>
    Task<CursorPage<PetDto>> ListAsync(
        string? search,
        Guid? tutorId,
        Cursor? after,
        int limit,
        CancellationToken cancellationToken = default);
}
