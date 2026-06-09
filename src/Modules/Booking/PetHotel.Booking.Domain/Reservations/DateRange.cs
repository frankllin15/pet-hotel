using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations;

/// <summary>Período da hospedagem (check-in/check-out). Imutável e auto-validado.</summary>
public sealed record DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static Result<DateRange> Create(DateOnly start, DateOnly end)
    {
        if (end <= start)
        {
            return Error.Validation("date_range.invalid", "A data de saída deve ser posterior à de entrada.");
        }

        return new DateRange(start, end);
    }

    /// <summary>Há sobreposição de períodos (meio-aberto: compartilhar a borda não conflita).</summary>
    public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;
}
