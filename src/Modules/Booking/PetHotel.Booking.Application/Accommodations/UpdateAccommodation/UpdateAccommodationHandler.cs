using FluentValidation;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Validation;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Accommodations.UpdateAccommodation;

/// <summary>Carrega a acomodação do tenant corrente, aplica a edição e reconcilia o status.</summary>
public static class UpdateAccommodationHandler
{
    public static async Task<Result> Handle(
        UpdateAccommodation command,
        IValidator<UpdateAccommodation> validator,
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

        var accommodation = await accommodations.FindAsync(new AccommodationId(command.Id), cancellationToken);
        if (accommodation is null)
        {
            return Error.NotFound("accommodation.not_found", "Acomodação não encontrada.");
        }

        var update = accommodation.Update(command.Name, command.DailyRate);
        if (update.IsFailure)
        {
            return update.Error;
        }

        // Reconcilia o status só quando muda (Activate/Deactivate conflitam se já no estado).
        if (command.Active && !accommodation.IsAvailable)
        {
            accommodation.Activate();
        }
        else if (!command.Active && accommodation.IsAvailable)
        {
            accommodation.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
