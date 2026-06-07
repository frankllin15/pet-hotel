using FluentValidation;

namespace PetHotel.Tenancy.Application.Users.RegisterUser;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RegisterUserValidator : AbstractValidator<RegisterUser>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).IsInEnum();
    }
}
