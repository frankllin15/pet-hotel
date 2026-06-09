using PetHotel.Registry.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.ListPets;

/// <summary>Valida o cursor/limite e delega para a porta de leitura (docs/04).</summary>
public static class ListPetsHandler
{
    private const int MaxLimit = 100;
    private const int DefaultLimit = 20;

    public static async Task<Result<CursorPage<PetDto>>> Handle(
        ListPets query,
        IPetQueries queries,
        CancellationToken cancellationToken)
    {
        Cursor? after = null;
        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            if (!Cursor.TryDecode(query.Cursor, out var decoded))
            {
                return Error.Validation("cursor.invalid", "Cursor de paginação inválido.");
            }

            after = decoded;
        }

        var limit = query.Limit <= 0 ? DefaultLimit : Math.Min(query.Limit, MaxLimit);

        return await queries.ListAsync(query.Search, query.TutorId, after, limit, cancellationToken);
    }
}
