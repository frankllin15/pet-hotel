namespace PetHotel.Operations.Application.Medications;

/// <summary>Projeção de leitura de uma administração de medicamento (docs/04).</summary>
public sealed record MedicationDto(
    Guid Id,
    string Drug,
    string Dose,
    DateTimeOffset AdministeredAt,
    string? GivenBy);

/// <summary>
/// Medicação do dia para o painel: traz a estadia (ContextId = ReservationId) para o front
/// resolver o pet a partir das reservas do dia.
/// </summary>
public sealed record DayMedicationDto(
    Guid Id,
    Guid ReservationId,
    string Drug,
    string Dose,
    DateTimeOffset AdministeredAt);
