using PetHotel.Registry.Domain.Tutors;

namespace PetHotel.Registry.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="Tutor"/>.</summary>
public interface ITutorRepository
{
    Task<Tutor?> FindAsync(TutorId id, CancellationToken cancellationToken = default);

    /// <summary>Existe no tenant corrente? (query filter aplicado.)</summary>
    Task<bool> ExistsAsync(TutorId id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

    void Add(Tutor tutor);

    void Remove(Tutor tutor);
}
