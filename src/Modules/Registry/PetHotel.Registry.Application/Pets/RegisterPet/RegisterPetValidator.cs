using FluentValidation;

namespace PetHotel.Registry.Application.Pets.RegisterPet;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RegisterPetValidator : AbstractValidator<RegisterPet>
{
    public RegisterPetValidator()
    {
        RuleFor(x => x.TutorId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Species).IsInEnum();
        RuleFor(x => x.Breed).MaximumLength(120);
        RuleFor(x => x.Size).IsInEnum().When(x => x.Size is not null);
        RuleFor(x => x.Sex).IsInEnum().When(x => x.Sex is not null);
        RuleFor(x => x.MicrochipCode).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
