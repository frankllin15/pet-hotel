using FluentValidation;

namespace PetHotel.Operations.Application.Incidents.ReportIncident;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class ReportIncidentValidator : AbstractValidator<ReportIncident>
{
    public ReportIncidentValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
