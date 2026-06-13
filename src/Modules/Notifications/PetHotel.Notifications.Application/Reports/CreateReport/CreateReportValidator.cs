using FluentValidation;

namespace PetHotel.Notifications.Application.Reports.CreateReport;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class CreateReportValidator : AbstractValidator<CreateReport>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.TutorId).NotEmpty();
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
    }
}
