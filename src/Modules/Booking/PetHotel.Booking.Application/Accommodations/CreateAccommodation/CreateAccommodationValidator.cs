using FluentValidation;

namespace PetHotel.Booking.Application.Accommodations.CreateAccommodation;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class CreateAccommodationValidator : AbstractValidator<CreateAccommodation>
{
    public CreateAccommodationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DailyRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
    }
}
