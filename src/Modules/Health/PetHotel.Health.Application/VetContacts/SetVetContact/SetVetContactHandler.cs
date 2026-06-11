using FluentValidation;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Application.Validation;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.VetContacts.SetVetContact;

/// <summary>
/// Define o veterinário particular no tenant corrente, criando a ficha do pet se
/// necessário (find-or-create do agregado raiz).
/// </summary>
public static class SetVetContactHandler
{
    public static async Task<Result> Handle(
        SetVetContact command,
        IValidator<SetVetContact> validator,
        ITenantContext tenantContext,
        IHealthRecordRepository healthRecords,
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

        var contact = VetContact.Create(command.Name, command.Phone, command.Clinic);
        if (contact.IsFailure)
        {
            return contact.Error;
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

        record.SetVetContact(contact.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
