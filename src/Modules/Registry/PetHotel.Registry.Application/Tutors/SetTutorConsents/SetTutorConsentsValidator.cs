using FluentValidation;

namespace PetHotel.Registry.Application.Tutors.SetTutorConsents;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class SetTutorConsentsValidator : AbstractValidator<SetTutorConsents>
{
    public SetTutorConsentsValidator()
    {
        RuleFor(x => x.TutorId).NotEmpty();
        RuleFor(x => x.Consents).NotNull();
        RuleForEach(x => x.Consents).ChildRules(consent =>
        {
            consent.RuleFor(c => c.Type).IsInEnum();
        });
    }
}
