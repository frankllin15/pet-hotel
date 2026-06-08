using FluentValidation;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Application.Invitations;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class InviteUserValidator : AbstractValidator<InviteUser>
{
    public InviteUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(Roles.IsValid)
            .WithMessage($"Papel inválido. Use um de: {string.Join(", ", Roles.All)}.");
    }
}
