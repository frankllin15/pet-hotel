namespace PetHotel.Operations.Application.Medications.RecordMedication;

/// <summary>Registra a administração de um medicamento numa estadia. AdministeredAt nulo = agora.</summary>
public sealed record RecordMedication(Guid ReservationId, string Drug, string Dose, DateTimeOffset? AdministeredAt);
