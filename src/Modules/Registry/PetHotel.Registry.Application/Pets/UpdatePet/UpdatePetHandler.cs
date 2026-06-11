using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.UpdatePet;

/// <summary>Carrega o pet do tenant corrente, aplica a edição e persiste (docs/03).</summary>
public static class UpdatePetHandler
{
    public static async Task<Result> Handle(
        UpdatePet command,
        IValidator<UpdatePet> validator,
        ITenantContext tenantContext,
        IPetRepository pets,
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

        var pet = await pets.FindAsync(new PetId(command.Id), cancellationToken);
        if (pet is null)
        {
            return Error.NotFound("pet.not_found", "Pet não encontrado neste hotel.");
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        // Monta o value object da rotina alimentar; rotina inválida aborta a edição.
        FeedingRoutine? feedingRoutine = null;
        if (command.FeedingRoutine is { } routineInput)
        {
            var routine = FeedingRoutine.Create(
                routineInput.FoodName,
                routineInput.PortionSize,
                routineInput.MealTimes,
                routineInput.Restrictions,
                routineInput.FoodSource);
            if (routine.IsFailure)
            {
                return routine.Error;
            }

            feedingRoutine = routine.Value;
        }

        var result = pet.Update(
            command.Name,
            command.Species,
            command.Breed,
            command.BirthDate,
            command.Size,
            command.Sex,
            command.Neutered,
            command.MicrochipCode,
            command.Notes,
            command.Sociability,
            command.Reactivity,
            command.Fear,
            command.Destructiveness,
            command.BehaviorNotes,
            today,
            feedingRoutine);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
