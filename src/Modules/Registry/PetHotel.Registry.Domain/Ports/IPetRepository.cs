using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Tutors;

namespace PetHotel.Registry.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="Pet"/>.</summary>
public interface IPetRepository
{
    Task<Pet?> FindAsync(PetId id, CancellationToken cancellationToken = default);

    /// <summary>O tutor possui algum pet no tenant corrente? (impede excluir tutor com pets.)</summary>
    Task<bool> ExistsByTutorAsync(TutorId tutorId, CancellationToken cancellationToken = default);

    void Add(Pet pet);

    void Remove(Pet pet);
}
