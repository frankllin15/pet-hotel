using PetHotel.Api.Http;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Accommodations;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Application.Accommodations.CreateAccommodation;
using PetHotel.Booking.Application.Accommodations.ListAccommodations;
using PetHotel.Booking.Application.Reservations.CancelReservation;
using PetHotel.Booking.Application.Reservations.CheckInReservation;
using PetHotel.Booking.Application.Reservations.CheckOutReservation;
using PetHotel.Booking.Application.Reservations.ConfirmReservation;
using PetHotel.Booking.Application.Reservations.CreateReservation;
using PetHotel.Booking.Application.Reservations.GetOccupancy;
using PetHotel.Booking.Application.Reservations.ListReservations;
using PetHotel.Booking.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Booking: acomodações, reservas (criar/confirmar/cancelar) e
/// calendário de ocupação. Restritos a usuários autenticados (tenant do token).
/// </summary>
public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Booking").RequireAuthorization();

        group.MapPost("/accommodations", async (CreateAccommodation command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/accommodations/{id}", new CreatedResponse(id)));
            })
            .WithName("CreateAccommodation")
            .WithSummary("Cria uma acomodação (unidade reservável).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/accommodations", async (IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<AccommodationDto>>>(new ListAccommodations(), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListAccommodations")
            .WithSummary("Lista as acomodações do tenant corrente.")
            .Produces<IReadOnlyList<AccommodationDto>>(StatusCodes.Status200OK);

        group.MapPost("/reservations", async (CreateReservation command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/reservations/{id}", new CreatedResponse(id)));
            })
            .WithName("CreateReservation")
            .WithSummary("Solicita uma reserva (estado Requested).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Cross-módulo (Booking + gateway p/ Health) → invocado direto via DI, fora do Wolverine.
        group.MapPost("/reservations/{id:guid}/confirm",
                async (
                    Guid id,
                    IReservationRepository reservations,
                    IAccommodationRepository accommodations,
                    IHealthClearanceGateway healthClearance,
                    IUnitOfWork unitOfWork,
                    TimeProvider clock,
                    CancellationToken ct) =>
                {
                    var result = await ConfirmReservationHandler.Handle(
                        new ConfirmReservation(id), reservations, accommodations, healthClearance, unitOfWork, clock, ct);
                    return result.ToHttpResult(Results.NoContent());
                })
            .WithName("ConfirmReservation")
            .WithSummary("Confirma a reserva (bloqueia se vacina obrigatória ausente/vencida).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/reservations/{id:guid}/check-in", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new CheckInReservation(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("CheckInReservation")
            .WithSummary("Registra o check-in (entrada) de uma reserva confirmada.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/reservations/{id:guid}/check-out", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new CheckOutReservation(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("CheckOutReservation")
            .WithSummary("Registra o check-out (saída) de uma reserva em estadia.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/reservations/{id:guid}/cancel", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new CancelReservation(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("CancelReservation")
            .WithSummary("Cancela a reserva.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations", async (string? status, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<ReservationDto>>>(new ListReservations(status), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListReservations")
            .WithSummary("Lista reservas do tenant corrente (filtro opcional por status).")
            .Produces<IReadOnlyList<ReservationDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/occupancy", async (DateOnly from, DateOnly to, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<OccupancyEntryDto>>>(new GetOccupancy(from, to), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetOccupancy")
            .WithSummary("Calendário de ocupação (reservas confirmadas) no intervalo.")
            .Produces<IReadOnlyList<OccupancyEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
