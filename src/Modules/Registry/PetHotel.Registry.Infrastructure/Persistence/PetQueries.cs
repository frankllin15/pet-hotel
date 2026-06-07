using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Pets;
using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>Lado de leitura de pets (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class PetQueries(RegistryDbContext dbContext) : IPetQueries
{
    public async Task<PetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await dbContext.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == new PetId(id), cancellationToken);

        return pet is null
            ? null
            : new PetDto(
                pet.Id.Value,
                pet.TutorId.Value,
                pet.Name,
                pet.Species.ToString(),
                pet.Breed,
                pet.BirthDate,
                pet.Notes,
                pet.CreatedAt);
    }
}
