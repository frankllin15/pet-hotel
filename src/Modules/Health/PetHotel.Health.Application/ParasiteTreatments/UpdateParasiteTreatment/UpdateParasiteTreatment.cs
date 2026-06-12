using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Application.ParasiteTreatments.UpdateParasiteTreatment;

/// <summary>Edita um controle de parasitas existente na ficha de um pet.</summary>
public sealed record UpdateParasiteTreatment(
    Guid PetId,
    Guid TreatmentId,
    ParasiteTreatmentType Type,
    string? ProductName,
    DateOnly AppliedOn,
    DateOnly? NextDueOn);
