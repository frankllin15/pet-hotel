using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;

namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Valida credenciais e emite o JWT. Anônimo.</summary>
public static class LoginHandler
{
    public static async Task<Result<AccessToken>> Handle(
        Login command,
        IValidator<Login> validator,
        IUserAccountService userAccounts,
        IJwtTokenIssuer tokenIssuer,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var auth = await userAccounts.AuthenticateAsync(command.Email, command.Password, cancellationToken);
        if (auth.IsFailure)
        {
            return auth.Error;
        }

        return tokenIssuer.Issue(auth.Value);
    }
}
