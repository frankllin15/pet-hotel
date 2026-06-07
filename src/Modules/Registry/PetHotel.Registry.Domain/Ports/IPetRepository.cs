using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="Pet"/>.</summary>
public interface IPetRepository
{
    Task<Pet?> FindAsync(PetId id, CancellationToken cancellationToken = default);

    void Add(Pet pet);
}
