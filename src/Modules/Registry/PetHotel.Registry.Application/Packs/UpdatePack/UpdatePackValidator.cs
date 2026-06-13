using FluentValidation;

namespace PetHotel.Registry.Application.Packs.UpdatePack;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdatePackValidator : AbstractValidator<UpdatePack>
{
    public UpdatePackValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleForEach(x => x.MemberPetIds).NotEmpty();
    }
}
