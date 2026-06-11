using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Tutors.UpdateTutor;

/// <summary>
/// Carrega o tutor do tenant corrente, aplica a edição e persiste. Garante unicidade
/// de e-mail por hotel quando o e-mail muda (ignorando o próprio tutor, docs/03).
/// </summary>
public static class UpdateTutorHandler
{
    public static async Task<Result> Handle(
        UpdateTutor command,
        IValidator<UpdateTutor> validator,
        ITenantContext tenantContext,
        ITutorRepository tutors,
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

        var tutor = await tutors.FindAsync(new TutorId(command.Id), cancellationToken);
        if (tutor is null)
        {
            return Error.NotFound("tutor.not_found", "Tutor não encontrado neste hotel.");
        }

        // Monta os value objects das coleções; o primeiro inválido aborta a edição.
        var emergencyContacts = new List<EmergencyContact>();
        foreach (var input in command.EmergencyContacts ?? [])
        {
            var contact = EmergencyContact.Create(input.Name, input.Phone, input.Relationship);
            if (contact.IsFailure)
            {
                return contact.Error;
            }

            emergencyContacts.Add(contact.Value);
        }

        var authorizedPickups = new List<AuthorizedPickup>();
        foreach (var input in command.AuthorizedPickups ?? [])
        {
            var pickup = AuthorizedPickup.Create(input.Name, input.Document);
            if (pickup.IsFailure)
            {
                return pickup.Error;
            }

            authorizedPickups.Add(pickup.Value);
        }

        // Unicidade de e-mail só importa quando o e-mail muda (compara normalizado).
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        if (emailResult.Value.Value != tutor.Email.Value
            && await tutors.EmailExistsAsync(emailResult.Value, cancellationToken))
        {
            return Error.Conflict("tutor.email_taken", "Já existe um tutor com esse e-mail neste hotel.");
        }

        var result = tutor.Update(command.FullName, command.Email, command.Phone, emergencyContacts, authorizedPickups);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
