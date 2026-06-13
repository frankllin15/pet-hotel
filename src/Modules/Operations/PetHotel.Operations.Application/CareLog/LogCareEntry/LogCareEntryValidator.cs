using FluentValidation;

namespace PetHotel.Operations.Application.CareLog.LogCareEntry;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class LogCareEntryValidator : AbstractValidator<LogCareEntry>
{
    public LogCareEntryValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
