using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.DeletePet;

/// <summary>
/// Carrega o pet do tenant corrente e o remove. Devolve a chave da foto (ou null) para
/// o endpoint apagar o arquivo no storage.
/// </summary>
public static class DeletePetHandler
{
    public static async Task<Result<string?>> Handle(
        DeletePet command,
        ITenantContext tenantContext,
        IPetRepository pets,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var pet = await pets.FindAsync(new PetId(command.Id), cancellationToken);
        if (pet is null)
        {
            return Error.NotFound("pet.not_found", "Pet não encontrado neste hotel.");
        }

        var photoKey = pet.PhotoKey;

        pet.Delete();
        pets.Remove(pet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return photoKey;
    }
}
