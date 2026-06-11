using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Application.ParasiteTreatments.RegisterParasiteTreatment;

/// <summary>Registra um controle de parasitas na ficha de um pet (cria a ficha se ainda não existir).</summary>
public sealed record RegisterParasiteTreatment(
    Guid PetId,
    ParasiteTreatmentType Type,
    string? ProductName,
    DateOnly AppliedOn,
    DateOnly? NextDueOn);
