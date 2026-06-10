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

    private static readonly DateTimeOffset Now = new(2026, 6, 10, 9, 0, 0, TimeSpan.Zero);

    private static Reservation NewConfirmed()
    {
        var reservation = NewRequested();
        reservation.Confirm(isPetHealthCleared: true);
        return reservation;
    }

    [Fact]
    public void Checkin_de_reserva_confirmada_inicia_estadia_e_levanta_evento()
    {
        var reservation = NewConfirmed();

        var result = reservation.CheckIn(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
        Assert.Equal(Now, reservation.CheckedInAt);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationCheckedIn);
    }

    [Fact]
    public void Checkin_de_reserva_nao_confirmada_bloqueia()
    {
        var reservation = NewRequested();

        var result = reservation.CheckIn(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("reservation.invalid_state", result.Error.Code);
        Assert.Equal(ReservationStatus.Requested, reservation.Status);
    }

    [Fact]
    public void Checkout_apos_checkin_encerra_estadia_e_levanta_evento()
    {
        var reservation = NewConfirmed();
        reservation.CheckIn(Now);

        var result = reservation.CheckOut(Now.AddDays(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.CheckedOut, reservation.Status);
        Assert.Equal(Now.AddDays(2), reservation.CheckedOutAt);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationCheckedOut);
    }

    [Fact]
    public void Checkout_sem_checkin_bloqueia()
    {
        var reservation = NewConfirmed();

        var result = reservation.CheckOut(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("reservation.invalid_state", result.Error.Code);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Cancelar_apos_checkin_bloqueia()
    {
        var reservation = NewConfirmed();
        reservation.CheckIn(Now);

        var result = reservation.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal("reservation.invalid_state", result.Error.Code);
        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
    }
}
