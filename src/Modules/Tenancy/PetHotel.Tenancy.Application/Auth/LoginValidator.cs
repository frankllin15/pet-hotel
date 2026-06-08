using FluentValidation;

namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
