using FluentValidation;

namespace PetHotel.Health.Application.ParasiteTreatments.UpdateParasiteTreatment;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdateParasiteTreatmentValidator : AbstractValidator<UpdateParasiteTreatment>
{
    public UpdateParasiteTreatmentValidator()
    {
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.TreatmentId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ProductName).MaximumLength(200);
        RuleFor(x => x.NextDueOn).GreaterThan(x => x.AppliedOn).When(x => x.NextDueOn is not null);
    }
}
