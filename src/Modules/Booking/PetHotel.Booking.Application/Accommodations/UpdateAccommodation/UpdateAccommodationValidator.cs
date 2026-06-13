using FluentValidation;

namespace PetHotel.Booking.Application.Accommodations.UpdateAccommodation;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdateAccommodationValidator : AbstractValidator<UpdateAccommodation>
{
    public UpdateAccommodationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DailyRate).GreaterThanOrEqualTo(0);
    }
}
