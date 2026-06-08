using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;
using PetHotel.Tenancy.Domain.Configuration;
using PetHotel.Tenancy.Domain.Ports;

namespace PetHotel.Tenancy.Application.Configuration;

/// <summary>Aplica o wizard de setup à configuração do tenant corrente.</summary>
public static class UpdateTenantConfigurationHandler
{
    public static async Task<Result> Handle(
        UpdateTenantConfiguration command,
        IValidator<UpdateTenantConfiguration> validator,
        ITenantConfigurationRepository configurations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var config = await configurations.GetForCurrentTenantAsync(cancellationToken);
        if (config is null)
        {
            return Error.NotFound("configuration.not_found", "Configuração não encontrada.");
        }

        var accommodationTypes = new List<AccommodationType>();
        foreach (var input in command.AccommodationTypes)
        {
            var result = AccommodationType.Create(input.Name, input.Capacity, input.DailyPrice);
            if (result.IsFailure)
            {
                return result.Error;
            }

            accommodationTypes.Add(result.Value);
        }

        config.Update(accommodationTypes, command.RequiredVaccines, command.CheckInTime, command.CheckOutTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
