using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;

namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Ativa a conta (define a senha). Anônimo.</summary>
public static class ActivateAccountHandler
{
    public static async Task<Result> Handle(
        ActivateAccount command,
        IValidator<ActivateAccount> validator,
        IUserAccountService userAccounts,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        return await userAccounts.ActivateAsync(command.Email, command.Token, command.Password, cancellationToken);
    }
}
