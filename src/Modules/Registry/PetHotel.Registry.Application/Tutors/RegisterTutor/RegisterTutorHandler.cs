using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Tutors.RegisterTutor;

/// <summary>Cria um tutor no tenant corrente (resolvido pelo token, docs/04).</summary>
public static class RegisterTutorHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterTutor command,
        IValidator<RegisterTutor> validator,
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

        // Monta os value objects das coleções; o primeiro inválido aborta o cadastro.
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

        // Monta o value object do faturamento; faturamento inválido aborta o cadastro.
        BillingInfo? billing = null;
        if (command.Billing is { } billingInput)
        {
            var billingResult = BillingInfo.Create(
                billingInput.Document,
                billingInput.BillingEmail,
                billingInput.AddressLine1,
                billingInput.AddressLine2,
                billingInput.City,
                billingInput.State,
                billingInput.PostalCode);
            if (billingResult.IsFailure)
            {
                return billingResult.Error;
            }

            billing = billingResult.Value;
        }

        var result = Tutor.Register(
            tenantContext.Current, command.FullName, command.Email, command.Phone, emergencyContacts, authorizedPickups, billing);
        if (result.IsFailure)
        {
            return result.Error;
        }

        var tutor = result.Value;

        if (await tutors.EmailExistsAsync(tutor.Email, cancellationToken))
        {
            return Error.Conflict("tutor.email_taken", "Já existe um tutor com esse e-mail neste hotel.");
        }

        tutors.Add(tutor);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tutor.Id.Value;
    }
}
