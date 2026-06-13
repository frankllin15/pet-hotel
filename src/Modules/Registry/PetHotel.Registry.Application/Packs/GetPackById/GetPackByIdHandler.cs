using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Packs;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Packs.GetPackById;

/// <summary>Projeta direto para DTO via porta de leitura (docs/04).</summary>
public static class GetPackByIdHandler
{
    public static async Task<Result<PackDto>> Handle(
        GetPackById query,
        IPackQueries queries,
        CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(query.Id, cancellationToken);

        return dto is null
            ? Error.NotFound("pack.not_found", "Matilha não encontrada.")
            : dto;
    }
}
