using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.UnitTests;

public class DateRangeTests
{
    private static DateRange Range(int startDay, int endDay) =>
        DateRange.Create(new DateOnly(2026, 6, startDay), new DateOnly(2026, 6, endDay)).Value;

    [Fact]
    public void Criar_com_fim_antes_ou_igual_ao_inicio_falha()
    {
        var result = DateRange.Create(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 10));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Periodos_que_se_cruzam_sobrepoem()
    {
        Assert.True(Range(10, 14).Overlaps(Range(12, 16)));
    }

    [Fact]
    public void Periodos_que_compartilham_a_borda_nao_sobrepoem()
    {
        // [10,12) e [12,14): check-out no mesmo dia do próximo check-in não conflita.
        Assert.False(Range(10, 12).Overlaps(Range(12, 14)));
    }

    [Fact]
    public void Periodos_disjuntos_nao_sobrepoem()
    {
        Assert.False(Range(10, 12).Overlaps(Range(20, 22)));
    }
}
