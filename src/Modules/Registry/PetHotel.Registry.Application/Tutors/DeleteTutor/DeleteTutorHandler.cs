using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Tutors.DeleteTutor;

/// <summary>
/// Remove o tutor do tenant corrente. Bloqueia (Conflict) se ele ainda tiver pets —
/// o usuário precisa excluir/transferir os pets antes.
/// </summary>
public static class DeleteTutorHandler
{
    public static async Task<Result> Handle(
        DeleteTutor command,
        ITenantContext tenantContext,
        ITutorRepository tutors,
        IPetRepository pets,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var tutor = await tutors.FindAsync(new TutorId(command.Id), cancellationToken);
        if (tutor is null)
        {
            return Error.NotFound("tutor.not_found", "Tutor não encontrado neste hotel.");
        }

        if (await pets.ExistsByTutorAsync(tutor.Id, cancellationToken))
        {
            return Error.Conflict(
                "tutor.has_pets", "Não é possível excluir um tutor com pets vinculados. Exclua os pets primeiro.");
        }

        tutor.Delete();
        tutors.Remove(tutor);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
