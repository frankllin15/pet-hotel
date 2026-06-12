using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.SetPetPhoto;

/// <summary>
/// Carrega o pet do tenant corrente e atualiza a referência da foto. Devolve a chave
/// anterior (ou null) para o endpoint apagar o arquivo substituído/removido.
/// </summary>
public static class SetPetPhotoHandler
{
    public static async Task<Result<string?>> Handle(
        SetPetPhoto command,
        ITenantContext tenantContext,
        IPetRepository pets,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var pet = await pets.FindAsync(new PetId(command.PetId), cancellationToken);
        if (pet is null)
        {
            return Error.NotFound("pet.not_found", "Pet não encontrado neste hotel.");
        }

        var previousKey = pet.SetPhoto(command.PhotoKey);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return previousKey;
    }
}
