using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;

namespace PetHotel.Registry.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Tutor"/>. Leituras já filtradas por tenant.</summary>
public sealed class TutorRepository(RegistryDbContext dbContext) : ITutorRepository
{
    public Task<Tutor?> FindAsync(TutorId id, CancellationToken cancellationToken = default) =>
        dbContext.Tutors.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(TutorId id, CancellationToken cancellationToken = default) =>
        dbContext.Tutors.AnyAsync(t => t.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default) =>
        dbContext.Tutors.AnyAsync(t => t.Email == email, cancellationToken);

    public void Add(Tutor tutor) => dbContext.Tutors.Add(tutor);

    public void Remove(Tutor tutor) => dbContext.Tutors.Remove(tutor);
}
