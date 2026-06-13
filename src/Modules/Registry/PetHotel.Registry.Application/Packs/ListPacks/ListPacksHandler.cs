using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Packs;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Packs.ListPacks;

/// <summary>Delega para a porta de leitura (docs/04).</summary>
public static class ListPacksHandler
{
    public static async Task<Result<IReadOnlyList<PackSummaryDto>>> Handle(
        ListPacks query,
        IPackQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.ListAsync(cancellationToken));
    }
}
