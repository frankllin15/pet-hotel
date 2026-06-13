using FluentValidation;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Validation;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Application.Tutors.SetTutorConsents;

/// <summary>
/// Aplica as decisões de consentimento ao tutor do tenant corrente, carimbando o momento
/// e a versão dos termos vigente. Decisões inalteradas preservam o registro original.
/// </summary>
public static class SetTutorConsentsHandler
{
    /// <summary>
    /// Versão dos termos/política de privacidade vigente. Constante no MVP; pode migrar
    /// para a configuração do tenant quando houver versionamento real dos termos.
    /// </summary>
    private const string CurrentTermsVersion = "1.0";

    public static async Task<Result> Handle(
        SetTutorConsents command,
        IValidator<SetTutorConsents> validator,
        ITenantContext tenantContext,
        ITutorRepository tutors,
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

        var tutor = await tutors.FindAsync(new TutorId(command.TutorId), cancellationToken);
        if (tutor is null)
        {
            return Error.NotFound("tutor.not_found", "Tutor não encontrado neste hotel.");
        }

        var now = clock.GetUtcNow();
        foreach (var decision in command.Consents)
        {
            var result = tutor.SetConsent(decision.Type, decision.Granted, now, CurrentTermsVersion);
            if (result.IsFailure)
            {
                return result.Error;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
