using FluentValidation;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Application.Validation;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.ParasiteTreatments.UpdateParasiteTreatment;

/// <summary>Carrega a ficha do pet no tenant corrente e edita o controle de parasitas informado.</summary>
public static class UpdateParasiteTreatmentHandler
{
    public static async Task<Result> Handle(
        UpdateParasiteTreatment command,
        IValidator<UpdateParasiteTreatment> validator,
        ITenantContext tenantContext,
        IHealthRecordRepository healthRecords,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
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

        var record = await healthRecords.FindByPetAsync(new PetReference(command.PetId), cancellationToken);
        if (record is null)
        {
            return Error.NotFound("health.not_found", "Ficha de saúde não encontrada para o pet.");
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var result = record.UpdateParasiteTreatment(
            new ParasiteTreatmentId(command.TreatmentId),
            command.Type,
            command.ProductName,
            command.AppliedOn,
            command.NextDueOn,
            today);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
