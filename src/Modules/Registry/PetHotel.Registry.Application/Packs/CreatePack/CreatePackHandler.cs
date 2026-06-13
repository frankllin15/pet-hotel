using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Packs.CreatePack;

/// <summary>Cria a matilha delegando as invariantes ao agregado (docs/03).</summary>
public static class CreatePackHandler
{
    public static async Task<Result<Guid>> Handle(
        CreatePack command,
        IValidator<CreatePack> validator,
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

        var members = (command.MemberPetIds ?? []).Select(id => new PetId(id));
        var result = Pack.Create(tenantContext.Current, command.Name, command.Notes, members);
        if (result.IsFailure)
        {
            return result.Error;
        }

        packs.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
