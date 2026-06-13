using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.CareLog.AddCareEntryPhoto;

/// <summary>Registra a chave da foto na ocorrência (toca só o OperationsDbContext → via bus).</summary>
public static class AddCareEntryPhotoHandler
{
    public static async Task<Result> Handle(
        AddCareEntryPhoto command,
        ICareLogRepository entries,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var entry = await entries.FindAsync(new CareLogEntryId(command.EntryId), cancellationToken);
        if (entry is null)
        {
            return Error.NotFound("care_log.not_found", "Ocorrência não encontrada.");
        }

        var result = entry.AddPhoto(command.Key);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
