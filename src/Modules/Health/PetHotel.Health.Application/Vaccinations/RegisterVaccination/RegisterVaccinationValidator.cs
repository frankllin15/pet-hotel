using FluentValidation;

namespace PetHotel.Health.Application.Vaccinations.RegisterVaccination;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RegisterVaccinationValidator : AbstractValidator<RegisterVaccination>
{
    public RegisterVaccinationValidator()
    {
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ExpiresOn).GreaterThan(x => x.AppliedOn);
    }
}
