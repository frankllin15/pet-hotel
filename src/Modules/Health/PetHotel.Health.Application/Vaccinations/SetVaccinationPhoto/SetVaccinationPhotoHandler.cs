using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.Vaccinations.SetVaccinationPhoto;

/// <summary>
/// Carrega a ficha do pet no tenant corrente e atualiza a foto da vacinação. Devolve a
/// chave anterior (ou null) para o endpoint apagar o arquivo substituído/removido.
/// </summary>
public static class SetVaccinationPhotoHandler
{
    public static async Task<Result<string?>> Handle(
        SetVaccinationPhoto command,
        ITenantContext tenantContext,
        IHealthRecordRepository healthRecords,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var record = await healthRecords.FindByPetAsync(new PetReference(command.PetId), cancellationToken);
        if (record is null)
        {
            return Error.NotFound("health.not_found", "Ficha de saúde não encontrada para o pet.");
        }

        var result = record.SetVaccinationPhoto(new VaccinationId(command.VaccinationId), command.PhotoKey);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result.Value;
    }
}
