using FluentValidation;

namespace PetHotel.Health.Application.VetContacts.SetVetContact;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class SetVetContactValidator : AbstractValidator<SetVetContact>
{
    public SetVetContactValidator()
    {
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Clinic).MaximumLength(200);
    }
}
