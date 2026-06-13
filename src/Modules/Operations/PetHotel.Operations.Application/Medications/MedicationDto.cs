namespace PetHotel.Operations.Application.Medications;

/// <summary>Projeção de leitura de uma administração de medicamento (docs/04).</summary>
public sealed record MedicationDto(
    Guid Id,
    string Drug,
    string Dose,
    DateTimeOffset AdministeredAt,
    string? GivenBy);
