using FluentValidation;

namespace PetHotel.Operations.Application.Medications.RecordMedication;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RecordMedicationValidator : AbstractValidator<RecordMedication>
{
    public RecordMedicationValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Drug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dose).NotEmpty().MaximumLength(100);
    }
}
