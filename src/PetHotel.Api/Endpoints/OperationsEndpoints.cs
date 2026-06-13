using FluentValidation;
using PetHotel.Api.Http;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.CareLog;
using PetHotel.Operations.Application.CareLog.GetStayCareLog;
using PetHotel.Operations.Application.CareLog.LogCareEntry;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Operations: diário de bordo por estadia (registro + timeline).
/// O diário é vinculado a uma estadia (reserva) — registrar exige check-in feito.
/// </summary>
public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Operations").RequireAuthorization();

        // Cross-módulo (Operations grava + Booking lê via gateway) → invocado direto via DI,
        // fora do Wolverine (mesmo motivo do ConfirmReservation: dois DbContexts).
        group.MapPost("/reservations/{reservationId:guid}/care-log", async (
                Guid reservationId,
                LogCareEntry command,
                IValidator<LogCareEntry> validator,
                ITenantContext tenantContext,
                IStayGateway stays,
                ICareLogRepository entries,
                IUnitOfWork unitOfWork,
                TimeProvider clock,
                CancellationToken ct) =>
            {
                var result = await LogCareEntryHandler.Handle(
                    command with { ReservationId = reservationId },
                    validator, tenantContext, stays, entries, unitOfWork, clock, ct);
                return result.ToHttpResult(id =>
                    Results.Created($"/v1/reservations/{reservationId}/care-log/{id}", new CreatedResponse(id)));
            })
            .WithName("LogCareEntry")
            .WithSummary("Registra uma ocorrência no diário de bordo da estadia.")
            .WithDescription("Exige estadia com check-in feito (409 se o pet não chegou). OccurredAt opcional (default: agora).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations/{reservationId:guid}/care-log", async (
                Guid reservationId, string? cursor, int? limit, IMessageBus bus, CancellationToken ct) =>
            {
                var query = new GetStayCareLog(reservationId, cursor, limit ?? 20);
                var result = await bus.InvokeAsync<Result<CursorPage<CareLogEntryDto>>>(query, ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetStayCareLog")
            .WithSummary("Timeline do diário de bordo da estadia (paginada por cursor).")
            .Produces<CursorPage<CareLogEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
