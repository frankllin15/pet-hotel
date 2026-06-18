using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace PetHotel.Api.Observability;

/// <summary>
/// Readiness check do Postgres (docs/05 §Observabilidade): abre uma conexão do pool
/// compartilhado e executa <c>SELECT 1</c>. Cobre a conectividade do banco e, por
/// tabela, do <b>Outbox</b> do Wolverine (que vive no mesmo Postgres). Marcado com a
/// tag <c>ready</c> e exposto em <c>/ready</c> — separado do liveness <c>/health</c>.
/// </summary>
public sealed class PostgresHealthCheck(NpgsqlDataSource dataSource) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres indisponível.", ex);
        }
    }
}
