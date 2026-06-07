using FluentValidation;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Application.Validation;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.Vaccinations.RegisterVaccination;

/// <summary>
/// Registra a vacinação no tenant corrente, criando a ficha do pet se necessário
/// (find-or-create do agregado raiz).
/// </summary>
public static class RegisterVaccinationHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterVaccination command,
        IValidator<RegisterVaccination> validator,
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

        var pet = new PetReference(command.PetId);

        var record = await healthRecords.FindByPetAsync(pet, cancellationToken);
        if (record is null)
        {
            var opened = HealthRecord.Open(tenantContext.Current, pet);
            if (opened.IsFailure)
            {
                return opened.Error;
            }

            record = opened.Value;
            healthRecords.Add(record);
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var result = record.AddVaccination(command.Type, command.AppliedOn, command.ExpiresOn, today);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Value;
    }
}
