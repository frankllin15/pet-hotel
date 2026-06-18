using System.Diagnostics;
using Serilog.Context;

namespace PetHotel.Api.Observability;

/// <summary>
/// Estabelece o correlation id da requisição (header <c>X-Correlation-Id</c>) e o
/// propaga para as três pontas de observabilidade (docs/10):
/// <list type="bullet">
///   <item>Serilog: enriquece <b>todos</b> os logs da request via <c>LogContext</c>.</item>
///   <item>OpenTelemetry: carimba o id como atributo do trace atual.</item>
///   <item>Sentry: marca o id no escopo (no-op se o Sentry estiver desligado).</item>
/// </list>
/// O id volta no header da resposta para o cliente (front) correlacionar. Se o
/// cliente não enviar o header, geramos um — assim toda request tem rastreio.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString();

        // Devolve no header da resposta (antes do corpo começar a ser escrito).
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Trace (OpenTelemetry) e Sentry — ambos no-op quando desligados.
        Activity.Current?.SetTag("correlation_id", correlationId);
        SentrySdk.ConfigureScope(scope => scope.SetTag("correlation_id", correlationId));

        // Enriquece todo log estruturado emitido durante a request.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
