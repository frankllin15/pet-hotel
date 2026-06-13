using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.CareLog.RemoveCareEntryPhoto;

/// <summary>Remove a chave da foto da ocorrência (o arquivo é apagado pelo endpoint).</summary>
public static class RemoveCareEntryPhotoHandler
{
    public static async Task<Result> Handle(
        RemoveCareEntryPhoto command,
        ICareLogRepository entries,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var entry = await entries.FindAsync(new CareLogEntryId(command.EntryId), cancellationToken);
        if (entry is null)
        {
            return Error.NotFound("care_log.not_found", "Ocorrência não encontrada.");
        }

        var result = entry.RemovePhoto(command.Key);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
