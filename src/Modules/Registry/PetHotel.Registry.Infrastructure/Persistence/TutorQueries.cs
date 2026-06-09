using Microsoft.EntityFrameworkCore;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Tutors;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>Lado de leitura de tutores (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class TutorQueries(RegistryDbContext dbContext) : ITutorQueries
{
    public async Task<TutorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tutor = await dbContext.Tutors
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == new TutorId(id), cancellationToken);

        return tutor is null ? null : ToDto(tutor);
    }

    public async Task<CursorPage<TutorDto>> ListAsync(
        string? search,
        Cursor? after,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tutors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ILike = case-insensitive (Postgres). Filtra por nome.
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.FullName, pattern));
        }

        // Keyset por CreatedAt desc (mais recentes primeiro); Id desc desempata a ordem.
        if (after is { } cursor)
        {
            query = query.Where(t => t.CreatedAt < cursor.Timestamp);
        }

        var rows = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).Select(ToDto).ToList();
        var next = hasMore && items.Count > 0
            ? new Cursor(items[^1].CreatedAt, items[^1].Id).Encode()
            : null;

        return new CursorPage<TutorDto>(items, next);
    }

    private static TutorDto ToDto(Tutor tutor) =>
        new(tutor.Id.Value, tutor.FullName, tutor.Email.Value, tutor.Phone.Value, tutor.CreatedAt);
}
