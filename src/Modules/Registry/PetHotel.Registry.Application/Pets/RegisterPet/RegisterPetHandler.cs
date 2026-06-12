using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Pets.RegisterPet;

/// <summary>
/// Cria um pet. Verifica (consistência entre agregados) que o tutor existe no
/// tenant corrente antes de delegar a criação ao agregado Pet (docs/03).
/// </summary>
public static class RegisterPetHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterPet command,
        IValidator<RegisterPet> validator,
        ITenantContext tenantContext,
        ITutorRepository tutors,
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

        var tutorId = new TutorId(command.TutorId);
        if (!await tutors.ExistsAsync(tutorId, cancellationToken))
        {
            return Error.NotFound("tutor.not_found", "Tutor não encontrado neste hotel.");
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        // Monta o value object da rotina alimentar; rotina inválida aborta o cadastro.
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

        // Monta os pertences trazidos; o primeiro inválido aborta o cadastro.
        var belongings = new List<Belonging>();
        foreach (var input in command.Belongings ?? [])
        {
            var belonging = Belonging.Create(input.Name, input.Quantity, input.Notes);
            if (belonging.IsFailure)
            {
                return belonging.Error;
            }

            belongings.Add(belonging.Value);
        }

        var result = Pet.Register(
            tenantContext.Current,
            tutorId,
            command.Name,
            command.Species,
            command.Breed,
            command.BirthDate,
            command.Size,
            command.Sex,
            command.Neutered,
            command.MicrochipCode,
            command.Notes,
            today,
            feedingRoutine,
            belongings);

        if (result.IsFailure)
        {
            return result.Error;
        }

        pets.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
