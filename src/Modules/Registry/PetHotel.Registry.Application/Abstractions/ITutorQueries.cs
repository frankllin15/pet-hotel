using PetHotel.Registry.Application.Tutors;

namespace PetHotel.Registry.Application.Abstractions;

/// <summary>Porta de leitura de tutores (AsNoTracking, docs/04).</summary>
public interface ITutorQueries
{
    Task<TutorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
