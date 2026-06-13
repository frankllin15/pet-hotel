using PetHotel.Registry.Domain.Packs;

namespace PetHotel.Registry.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="Pack"/>.</summary>
public interface IPackRepository
{
    Task<Pack?> FindAsync(PackId id, CancellationToken cancellationToken = default);

    void Add(Pack pack);

    void Remove(Pack pack);
}
