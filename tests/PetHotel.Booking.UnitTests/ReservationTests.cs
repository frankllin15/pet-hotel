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
    private const decimal DailyRate = 150m;

    private static Reservation NewRequested() => Reservation.Request(Tenant, Pet, Accommodation, Period, DailyRate).Value;

    [Fact]
    public void Solicitar_reserva_valida_levanta_evento()
    {
        var result = Reservation.Request(Tenant, Pet, Accommodation, Period, DailyRate);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Requested, result.Value.Status);
        Assert.Contains(result.Value.DomainEvents, e => e is ReservationRequested);
    }

    [Fact]
    public void Solicitar_reserva_calcula_total_por_diaria_e_noites()
    {
        // Período de 2 noites (10→12) × 150 = 300.
        var result = Reservation.Request(Tenant, Pet, Accommodation, Period, DailyRate);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Period.Nights);
        Assert.Equal(150m, result.Value.DailyRate);
        Assert.Equal(300m, result.Value.TotalAmount);
    }

    [Fact]
    public void Solicitar_reserva_com_diaria_negativa_falha()
    {
        var result = Reservation.Request(Tenant, Pet, Accommodation, Period, -1m);

        Assert.True(result.IsFailure);
        Assert.Equal("reservation.daily_rate_invalid", result.Error.Code);
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
    public void Checkin_com_estado_de_chegada_guarda_o_estado()
    {
        var reservation = NewConfirmed();
        var arrival = ArrivalState.Create(8.5m, ArrivalCondition.MinorIssues, " agitado ").Value;

        var result = reservation.CheckIn(Now, arrival);

        Assert.True(result.IsSuccess);
        Assert.NotNull(reservation.ArrivalState);
        Assert.Equal(8.5m, reservation.ArrivalState!.WeightKg);
        Assert.Equal(ArrivalCondition.MinorIssues, reservation.ArrivalState.Condition);
        Assert.Equal("agitado", reservation.ArrivalState.Observations); // trim
    }

    [Fact]
    public void Checkin_sem_estado_de_chegada_deixa_nulo()
    {
        var reservation = NewConfirmed();

        var result = reservation.CheckIn(Now);

        Assert.True(result.IsSuccess);
        Assert.Null(reservation.ArrivalState);
    }

    [Fact]
    public void Estado_de_chegada_com_peso_nao_positivo_falha()
    {
        var result = ArrivalState.Create(0m, ArrivalCondition.Healthy, null);

        Assert.True(result.IsFailure);
        Assert.Equal("arrival_state.weight_invalid", result.Error.Code);
    }

    [Fact]
    public void Estado_de_chegada_com_condicao_invalida_falha()
    {
        var result = ArrivalState.Create(null, (ArrivalCondition)99, null);

        Assert.True(result.IsFailure);
        Assert.Equal("arrival_state.condition_invalid", result.Error.Code);
    }

    [Fact]
    public void Anexar_foto_de_chegada_antes_do_checkin_bloqueia()
    {
        var reservation = NewConfirmed();

        var result = reservation.AddArrivalPhoto("tenant/arrivals/x.png");

        Assert.True(result.IsFailure);
        Assert.Equal("reservation.not_arrived", result.Error.Code);
    }

    [Fact]
    public void Anexar_e_remover_foto_de_chegada_apos_checkin()
    {
        var reservation = NewConfirmed();
        reservation.CheckIn(Now);

        Assert.True(reservation.AddArrivalPhoto("k1").IsSuccess);
        Assert.Single(reservation.ArrivalPhotoKeys);
        Assert.True(reservation.RemoveArrivalPhoto("k1").IsSuccess);
        Assert.Empty(reservation.ArrivalPhotoKeys);
        Assert.True(reservation.RemoveArrivalPhoto("k1").IsFailure); // já removida
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
