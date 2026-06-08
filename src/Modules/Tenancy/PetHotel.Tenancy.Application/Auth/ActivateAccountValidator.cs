using FluentValidation;

namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class ActivateAccountValidator : AbstractValidator<ActivateAccount>
{
    public ActivateAccountValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
