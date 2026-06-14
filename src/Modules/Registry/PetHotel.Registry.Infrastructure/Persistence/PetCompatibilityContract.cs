using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Contracts;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>
/// Implementação do contrato público de compatibilidade (docs/01). Avalia a ficha comportamental
/// do pet via <see cref="PackCompatibility"/> — a mesma regra usada nas matilhas — no tenant
/// corrente (query filter aplicado).
/// </summary>
public sealed class PetCompatibilityContract(RegistryDbContext dbContext) : IPetCompatibilityContract
{
    public async Task<IReadOnlyList<PetCompatibility>> GetCompatibilityAsync(
        IReadOnlyCollection<Guid> petIds, CancellationToken cancellationToken = default)
    {
        if (petIds.Count == 0)
        {
            return [];
        }

        var ids = petIds.Distinct().Select(id => new PetId(id)).ToList();

        var pets = await dbContext.Pets
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Sociability, p.Reactivity })
            .ToListAsync(cancellationToken);

        return pets
            .Select(p => new PetCompatibility(
                p.Id.Value,
                p.Name,
                PackCompatibility.Evaluate(p.Sociability, p.Reactivity).Select(f => f.ToString()).ToList()))
            .ToList();
    }
}
