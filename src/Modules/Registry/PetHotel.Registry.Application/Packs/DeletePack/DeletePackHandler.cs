using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Packs.DeletePack;

/// <summary>Exclui a matilha (a remoção é só do agrupamento — não afeta os pets).</summary>
public static class DeletePackHandler
{
    public static async Task<Result> Handle(
        DeletePack command,
        ITenantContext tenantContext,
        IPackRepository packs,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var pack = await packs.FindAsync(new PackId(command.Id), cancellationToken);
        if (pack is null)
        {
            return Error.NotFound("pack.not_found", "Matilha não encontrada neste hotel.");
        }

        pack.Delete();
        packs.Remove(pack);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
