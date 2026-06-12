using FluentValidation;

namespace PetHotel.Health.Application.Vaccinations.UpdateVaccination;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdateVaccinationValidator : AbstractValidator<UpdateVaccination>
{
    public UpdateVaccinationValidator()
    {
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.VaccinationId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ExpiresOn).GreaterThan(x => x.AppliedOn);
    }
}
