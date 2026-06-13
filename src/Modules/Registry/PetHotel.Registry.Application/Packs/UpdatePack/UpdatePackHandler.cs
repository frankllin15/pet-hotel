using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Packs.UpdatePack;

/// <summary>Carrega a matilha do tenant corrente, aplica a edição e persiste (docs/03).</summary>
public static class UpdatePackHandler
{
    public static async Task<Result> Handle(
        UpdatePack command,
        IValidator<UpdatePack> validator,
        ITenantContext tenantContext,
        IPackRepository packs,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var pack = await packs.FindAsync(new PackId(command.Id), cancellationToken);
        if (pack is null)
        {
            return Error.NotFound("pack.not_found", "Matilha não encontrada neste hotel.");
        }

        var members = (command.MemberPetIds ?? []).Select(id => new PetId(id));
        var result = pack.Update(command.Name, command.Notes, members);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
