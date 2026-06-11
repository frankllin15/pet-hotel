using FluentValidation;

namespace PetHotel.Registry.Application.Pets.UpdatePet;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdatePetValidator : AbstractValidator<UpdatePet>
{
    public UpdatePetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Species).IsInEnum();
        RuleFor(x => x.Breed).MaximumLength(120);
        RuleFor(x => x.Size).IsInEnum().When(x => x.Size is not null);
        RuleFor(x => x.Sex).IsInEnum().When(x => x.Sex is not null);
        RuleFor(x => x.MicrochipCode).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Sociability).IsInEnum().When(x => x.Sociability is not null);
        RuleFor(x => x.Reactivity).IsInEnum().When(x => x.Reactivity is not null);
        RuleFor(x => x.Fear).IsInEnum().When(x => x.Fear is not null);
        RuleFor(x => x.Destructiveness).IsInEnum().When(x => x.Destructiveness is not null);
        RuleFor(x => x.BehaviorNotes).MaximumLength(2000);
    }
}
