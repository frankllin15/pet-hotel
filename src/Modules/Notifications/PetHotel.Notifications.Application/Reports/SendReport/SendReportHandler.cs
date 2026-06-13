using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Domain.Ports;
using PetHotel.Notifications.Domain.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Application.Reports.SendReport;

/// <summary>Carrega o relatório do tenant corrente e o marca como enviado (sem canal real por ora).</summary>
public static class SendReportHandler
{
    public static async Task<Result> Handle(
        SendReport command,
        ITenantContext tenantContext,
        IOutboundMessageRepository messages,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var message = await messages.FindAsync(new OutboundMessageId(command.ReportId), cancellationToken);
        if (message is null)
        {
            return Error.NotFound("report.not_found", "Relatório não encontrado.");
        }

        var result = message.MarkSent(clock.GetUtcNow());
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
