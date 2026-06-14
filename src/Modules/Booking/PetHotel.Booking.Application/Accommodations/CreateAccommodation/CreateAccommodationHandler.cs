using FluentValidation;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Validation;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Accommodations.CreateAccommodation;

/// <summary>Cria uma acomodação no tenant corrente.</summary>
public static class CreateAccommodationHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateAccommodation command,
        IValidator<CreateAccommodation> validator,
        ITenantContext tenantContext,
        IAccommodationRepository accommodations,
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

        var result = Accommodation.Create(tenantContext.Current, command.Name, command.DailyRate, command.Capacity);
        if (result.IsFailure)
        {
            return result.Error;
        }

        accommodations.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
