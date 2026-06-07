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

        var result = Pet.Register(
            tenantContext.Current,
            tutorId,
            command.Name,
            command.Species,
            command.Breed,
            command.BirthDate,
            command.Notes,
            today);

        if (result.IsFailure)
        {
            return result.Error;
        }

        pets.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
