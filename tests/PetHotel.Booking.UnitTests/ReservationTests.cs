using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.Booking.Domain.Reservations.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.UnitTests;

public class ReservationTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly AccommodationId Accommodation = AccommodationId.New();
    private static DateRange Period => DateRange.Create(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 12)).Value;

    private static Reservation NewRequested() => Reservation.Request(Tenant, Pet, Accommodation, Period).Value;

    [Fact]
    public void Solicitar_reserva_valida_levanta_evento()
    {
        var result = Reservation.Request(Tenant, Pet, Accommodation, Period);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Requested, result.Value.Status);
        Assert.Contains(result.Value.DomainEvents, e => e is ReservationRequested);
    }

    [Fact]
    public void Confirmar_sem_aptidao_sanitaria_bloqueia()
    {
        var reservation = NewRequested();

        var result = reservation.Confirm(isPetHealthCleared: false);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("vaccine.expired", result.Error.Code);
        Assert.Equal(ReservationStatus.Requested, reservation.Status);
    }

    [Fact]
    public void Confirmar_apto_confirma_e_levanta_evento()
    {
        var reservation = NewRequested();

        var result = reservation.Confirm(isPetHealthCleared: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationConfirmed);
    }

    [Fact]
    public void Confirmar_reserva_ja_confirmada_retorna_conflito_de_estado()
    {
        var reservation = NewRequested();
        reservation.Confirm(true);

        var again = reservation.Confirm(true);

        Assert.True(again.IsFailure);
        Assert.Equal("reservation.invalid_state", again.Error.Code);
    }

    [Fact]
    public void Cancelar_reserva()
    {
        var reservation = NewRequested();

        Assert.True(reservation.Cancel().IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.True(reservation.Cancel().IsFailure);
    }
}
