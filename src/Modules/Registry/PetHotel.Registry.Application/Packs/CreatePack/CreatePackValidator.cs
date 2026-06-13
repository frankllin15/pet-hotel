using FluentValidation;

namespace PetHotel.Registry.Application.Packs.CreatePack;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class CreatePackValidator : AbstractValidator<CreatePack>
{
    public CreatePackValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleForEach(x => x.MemberPetIds).NotEmpty();
    }
}
