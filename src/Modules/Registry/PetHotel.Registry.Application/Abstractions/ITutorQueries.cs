using PetHotel.Registry.Application.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Abstractions;

/// <summary>Porta de leitura de tutores (AsNoTracking, docs/04).</summary>
public interface ITutorQueries
{
    Task<TutorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lista tutores do tenant corrente, paginada por cursor (docs/04).</summary>
    Task<CursorPage<TutorDto>> ListAsync(
        string? search,
        Cursor? after,
        int limit,
        CancellationToken cancellationToken = default);
}
