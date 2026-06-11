using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Pets;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>Lado de leitura de pets (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class PetQueries(RegistryDbContext dbContext) : IPetQueries
{
    public async Task<PetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await dbContext.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == new PetId(id), cancellationToken);

        return pet is null ? null : ToDto(pet);
    }

    public async Task<CursorPage<PetDto>> ListAsync(
        string? search,
        Guid? tutorId,
        Cursor? after,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Pets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        if (tutorId is { } id)
        {
            query = query.Where(p => p.TutorId == new TutorId(id));
        }

        // Keyset por CreatedAt desc (mais recentes primeiro); Id desc desempata a ordem.
        if (after is { } cursor)
        {
            query = query.Where(p => p.CreatedAt < cursor.Timestamp);
        }

        var rows = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).Select(ToDto).ToList();
        var next = hasMore && items.Count > 0
            ? new Cursor(items[^1].CreatedAt, items[^1].Id).Encode()
            : null;

        return new CursorPage<PetDto>(items, next);
    }

    private static PetDto ToDto(Pet pet) =>
        new(
            pet.Id.Value,
            pet.TutorId.Value,
            pet.Name,
            pet.Species.ToString(),
            pet.Breed,
            pet.BirthDate,
            pet.Size?.ToString(),
            pet.Sex?.ToString(),
            pet.Neutered,
            pet.MicrochipCode,
            pet.Notes,
            pet.Sociability?.ToString(),
            pet.Reactivity?.ToString(),
            pet.Fear?.ToString(),
            pet.Destructiveness?.ToString(),
            pet.BehaviorNotes,
            pet.CreatedAt);
}
