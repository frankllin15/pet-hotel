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

        RuleFor(x => x.Billing!).ChildRules(billing =>
        {
            billing.RuleFor(b => b.Document).NotEmpty().MaximumLength(20);
            billing.RuleFor(b => b.BillingEmail).EmailAddress().When(b => !string.IsNullOrWhiteSpace(b.BillingEmail));
            billing.RuleFor(b => b.AddressLine1).MaximumLength(200);
            billing.RuleFor(b => b.AddressLine2).MaximumLength(200);
            billing.RuleFor(b => b.City).MaximumLength(100);
            billing.RuleFor(b => b.State).MaximumLength(50);
            billing.RuleFor(b => b.PostalCode).MaximumLength(15);
        }).When(x => x.Billing is not null);
    }
}
