using FluentValidation;

namespace PetHotel.Registry.Application.Tutors.RegisterTutor;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RegisterTutorValidator : AbstractValidator<RegisterTutor>
{
    public RegisterTutorValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
    }
}
