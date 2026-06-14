using PetHotel.Api.Http;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Application.Reservations.GetDayBoard;
using PetHotel.Health.Application.Vaccinations;
using PetHotel.Health.Application.Vaccinations.GetExpiringVaccinations;
using PetHotel.Operations.Application.Medications;
using PetHotel.Operations.Application.Medications.GetDayMedications;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Dashboards (painel do dia). Composição estilo BFF: o API agrega as fatias de leitura de
/// cada módulo (Booking, Operations, Health) via bus — nenhum módulo lê tabela de outro.
/// </summary>
public static class DashboardEndpoints
{
    /// <summary>Janela (em dias) para considerar uma vacina "vencendo" no alerta.</summary>
    private const int VaccineAlertWindowDays = 30;

    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Dashboards").RequireAuthorization();

        group.MapGet("/dashboard", async (DateOnly? date, IMessageBus bus, TimeProvider clock, CancellationToken ct) =>
            {
                var day = date ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

                var board = await bus.InvokeAsync<Result<DayBoardDto>>(new GetDayBoard(day), ct);
                if (board.IsFailure)
                {
                    return board.ToHttpResult(_ => Results.Empty);
                }

                var medications = await bus.InvokeAsync<Result<IReadOnlyList<DayMedicationDto>>>(
                    new GetDayMedications(day), ct);
                if (medications.IsFailure)
                {
                    return medications.ToHttpResult(_ => Results.Empty);
                }

                var vaccines = await bus.InvokeAsync<Result<IReadOnlyList<ExpiringVaccinationDto>>>(
                    new GetExpiringVaccinations(day, VaccineAlertWindowDays), ct);
                if (vaccines.IsFailure)
                {
                    return vaccines.ToHttpResult(_ => Results.Empty);
                }

                return Results.Ok(new DashboardResponse(day, board.Value, medications.Value, vaccines.Value));
            })
            .WithName("GetDashboard")
            .WithSummary("Painel do dia: chegadas/saídas, ocupação, medicações e vacinas vencendo.")
            .WithDescription("Parâmetro 'date' opcional (default = hoje, UTC). Compõe fatias de Booking, Operations e Health.")
            .Produces<DashboardResponse>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>Resposta consolidada do painel do dia.</summary>
    public sealed record DashboardResponse(
        DateOnly Date,
        DayBoardDto Board,
        IReadOnlyList<DayMedicationDto> Medications,
        IReadOnlyList<ExpiringVaccinationDto> ExpiringVaccinations);
}
