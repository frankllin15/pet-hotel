using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;

namespace PetHotel.Registry.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Pet"/>. Leituras já filtradas por tenant.</summary>
public sealed class PetRepository(RegistryDbContext dbContext) : IPetRepository
{
    public Task<Pet?> FindAsync(PetId id, CancellationToken cancellationToken = default) =>
        dbContext.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Pet pet) => dbContext.Pets.Add(pet);
}
