using PetHotel.Registry.Application.Pets;

namespace PetHotel.Registry.Application.Abstractions;

/// <summary>Porta de leitura de pets (AsNoTracking, docs/04).</summary>
public interface IPetQueries
{
    Task<PetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
