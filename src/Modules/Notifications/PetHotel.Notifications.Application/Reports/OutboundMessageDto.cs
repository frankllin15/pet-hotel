namespace PetHotel.Notifications.Application.Reports;

/// <summary>Projeção de leitura de um relatório/mensagem ao tutor (docs/04).</summary>
public sealed record OutboundMessageDto(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    Guid ReservationId,
    DateOnly ReportDate,
    string Title,
    string Content,
    string Status,
    DateTimeOffset? SentAt,
    string? CreatedBy,
    DateTimeOffset CreatedAt);
