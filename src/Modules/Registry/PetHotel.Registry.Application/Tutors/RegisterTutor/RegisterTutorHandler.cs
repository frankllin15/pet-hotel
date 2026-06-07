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

        var result = Tutor.Register(tenantContext.Current, command.FullName, command.Email, command.Phone);
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
