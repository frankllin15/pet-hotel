using PetHotel.Registry.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.GetPetById;

/// <summary>Projeta direto para DTO via porta de leitura (docs/04).</summary>
public static class GetPetByIdHandler
{
    public static async Task<Result<PetDto>> Handle(
        GetPetById query,
        IPetQueries queries,
        CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(query.Id, cancellationToken);

        return dto is null
            ? Error.NotFound("pet.not_found", "Pet não encontrado.")
            : dto;
    }
}
