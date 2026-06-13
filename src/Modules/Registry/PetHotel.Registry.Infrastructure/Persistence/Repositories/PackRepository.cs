using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Ports;

namespace PetHotel.Registry.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Pack"/>. Leituras já filtradas por tenant.</summary>
public sealed class PackRepository(RegistryDbContext dbContext) : IPackRepository
{
    public Task<Pack?> FindAsync(PackId id, CancellationToken cancellationToken = default) =>
        dbContext.Packs.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Pack pack) => dbContext.Packs.Add(pack);

    public void Remove(Pack pack) => dbContext.Packs.Remove(pack);
}
