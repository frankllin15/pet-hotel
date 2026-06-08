using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;

namespace PetHotel.Tenancy.Application.Invitations;

/// <summary>
/// Cria um usuário pendente no tenant corrente com o papel do convite. Aceitar o
/// convite (ActivateAccount) é o que materializa o usuário ativo (memória onboarding-and-auth).
/// </summary>
public static class InviteUserHandler
{
    public static async Task<Result<Invitation>> Handle(
        InviteUser command,
        IValidator<InviteUser> validator,
        ITenantContext tenantContext,
        IUserAccountService userAccounts,
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

        var pending = await userAccounts.CreatePendingAsync(
            tenantContext.Current, command.Email, command.DisplayName, command.Role, cancellationToken);
        if (pending.IsFailure)
        {
            return pending.Error;
        }

        return new Invitation(pending.Value.UserId, pending.Value.ActivationToken);
    }
}
