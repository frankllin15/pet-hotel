using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Tutors;
using PetHotel.Registry.Domain.Tutors;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>Lado de leitura de tutores (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class TutorQueries(RegistryDbContext dbContext) : ITutorQueries
{
    public async Task<TutorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tutor = await dbContext.Tutors
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == new TutorId(id), cancellationToken);

        return tutor is null
            ? null
            : new TutorDto(tutor.Id.Value, tutor.FullName, tutor.Email.Value, tutor.Phone.Value, tutor.CreatedAt);
    }
}
