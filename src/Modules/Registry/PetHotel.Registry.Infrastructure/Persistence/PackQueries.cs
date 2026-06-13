using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Packs;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>
/// Lado de leitura de matilhas. A compatibilidade é um join (mesmo módulo) entre os membros e
/// a avaliação comportamental dos pets, avaliado por <see cref="PackCompatibility"/> (docs/04).
/// </summary>
public sealed class PackQueries(RegistryDbContext dbContext) : IPackQueries
{
    public async Task<IReadOnlyList<PackSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var packs = await dbContext.Packs
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var flagsByPet = await LoadFlagsAsync(packs.SelectMany(p => p.Members), cancellationToken);

        return packs
            .Select(p => new PackSummaryDto(
                p.Id.Value,
                p.Name,
                p.Members.Count,
                p.Members.Any(m => flagsByPet.TryGetValue(m.PetId, out var f) && f.Count > 0)))
            .ToList();
    }

    public async Task<PackDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pack = await dbContext.Packs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == new PackId(id), cancellationToken);

        if (pack is null)
        {
            return null;
        }

        // Carrega os pets membros (nome/espécie/comportamento) para montar os membros e os alertas.
        var memberIds = pack.Members.Select(m => new PetId(m.PetId)).ToList();
        var pets = await dbContext.Pets
            .AsNoTracking()
            .Where(p => memberIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        var petById = pets.ToDictionary(p => p.Id.Value);

        var members = pack.Members
            .Select(m =>
            {
                if (!petById.TryGetValue(m.PetId, out var pet))
                {
                    return new PackMemberDto(m.PetId, "(removido)", null, Found: false, []);
                }

                var flags = PackCompatibility.Evaluate(pet.Sociability, pet.Reactivity)
                    .Select(f => f.ToString())
                    .ToList();
                return new PackMemberDto(m.PetId, pet.Name, pet.Species.ToString(), Found: true, flags);
            })
            .ToList();

        return new PackDto(
            pack.Id.Value,
            pack.Name,
            pack.Notes,
            members,
            members.Any(m => m.Flags.Count > 0),
            pack.CreatedAt);
    }

    /// <summary>Carrega, em uma consulta, os sinais de compatibilidade de todos os pets membros.</summary>
    private async Task<Dictionary<Guid, IReadOnlyList<PackCompatibilityFlag>>> LoadFlagsAsync(
        IEnumerable<PackMember> members, CancellationToken cancellationToken)
    {
        var ids = members.Select(m => new PetId(m.PetId)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var pets = await dbContext.Pets
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Sociability, p.Reactivity })
            .ToListAsync(cancellationToken);

        return pets.ToDictionary(p => p.Id.Value, p => PackCompatibility.Evaluate(p.Sociability, p.Reactivity));
    }
}
