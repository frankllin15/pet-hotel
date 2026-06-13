namespace PetHotel.Notifications.Application.Reports.CreateReport;

/// <summary>Cria (rascunho) um relatório ao tutor montado a partir do diário de uma estadia/dia.</summary>
public sealed record CreateReport(
    Guid TutorId,
    Guid PetId,
    Guid ReservationId,
    DateOnly ReportDate,
    string Title,
    string Content);
