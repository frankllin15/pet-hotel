using FluentValidation;

namespace PetHotel.Booking.Application.Reservations.CreateReservation;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class CreateReservationValidator : AbstractValidator<CreateReservation>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.AccommodationId).NotEmpty();
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn);
    }
}
