using PetHotel.Registry.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Tutors.GetTutorById;

/// <summary>Projeta direto para DTO via porta de leitura (docs/04).</summary>
public static class GetTutorByIdHandler
{
    public static async Task<Result<TutorDto>> Handle(
        GetTutorById query,
        ITutorQueries queries,
        CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(query.Id, cancellationToken);

        return dto is null
            ? Error.NotFound("tutor.not_found", "Tutor não encontrado.")
            : dto;
    }
}
