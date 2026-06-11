using FluentValidation;

namespace PetHotel.Registry.Application.Tutors.UpdateTutor;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdateTutorValidator : AbstractValidator<UpdateTutor>
{
    public UpdateTutorValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();

        RuleForEach(x => x.EmergencyContacts).ChildRules(contact =>
        {
            contact.RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
            contact.RuleFor(c => c.Phone).NotEmpty().MaximumLength(20);
            contact.RuleFor(c => c.Relationship).MaximumLength(100);
        });

        RuleForEach(x => x.AuthorizedPickups).ChildRules(pickup =>
        {
            pickup.RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
            pickup.RuleFor(p => p.Document).MaximumLength(50);
        });
    }
}
