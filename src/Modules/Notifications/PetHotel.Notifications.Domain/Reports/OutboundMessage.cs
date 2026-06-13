using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Domain.Reports;

/// <summary>
/// Mensagem ao tutor (relatório diário montado a partir do diário). Agregado tenant-scoped;
/// referencia tutor/pet/estadia por Id. O conteúdo é montado fora (a partir do diário) e aqui
/// é persistido, marcado como enviado e listado. Envio por canal real (WhatsApp/e-mail) é futuro.
/// </summary>
public sealed class OutboundMessage : AggregateRoot<OutboundMessageId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public TutorReference Tutor { get; private set; }
    public PetReference Pet { get; private set; }
    /// <summary>Estadia (reserva) de origem do relatório.</summary>
    public Guid ReservationId { get; private set; }
    /// <summary>Dia coberto pelo relatório.</summary>
    public DateOnly ReportDate { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public MessageStatus Status { get; private set; }
    /// <summary>Momento em que foi marcado como enviado/compartilhado. Nulo enquanto rascunho.</summary>
    public DateTimeOffset? SentAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private OutboundMessage() { } // EF

    private OutboundMessage(
        OutboundMessageId id, TenantId tenantId, TutorReference tutor, PetReference pet, Guid reservationId,
        DateOnly reportDate, string title, string content) : base(id)
    {
        TenantId = tenantId;
        Tutor = tutor;
        Pet = pet;
        ReservationId = reservationId;
        ReportDate = reportDate;
        Title = title;
        Content = content;
        Status = MessageStatus.Draft;
    }

    public static Result<OutboundMessage> CreateReport(
        TenantId tenantId, TutorReference tutor, PetReference pet, Guid reservationId,
        DateOnly reportDate, string? title, string? content)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("report.tenant_required", "Tenant é obrigatório.");
        }

        if (tutor.Value == Guid.Empty)
        {
            return Error.Validation("report.tutor_required", "Tutor (destinatário) é obrigatório.");
        }

        if (pet.Value == Guid.Empty || reservationId == Guid.Empty)
        {
            return Error.Validation("report.stay_required", "Pet e estadia são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Error.Validation("report.title_required", "Título do relatório é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Error.Validation("report.content_required", "Conteúdo do relatório é obrigatório.");
        }

        return new OutboundMessage(
            OutboundMessageId.New(), tenantId, tutor, pet, reservationId, reportDate, title.Trim(), content.Trim());
    }

    /// <summary>Marca como enviado/compartilhado (sem canal real por ora).</summary>
    public Result MarkSent(DateTimeOffset now)
    {
        if (Status == MessageStatus.Sent)
        {
            return Error.Conflict("report.already_sent", "O relatório já foi marcado como enviado.");
        }

        Status = MessageStatus.Sent;
        SentAt = now;
        return Result.Success();
    }
}
